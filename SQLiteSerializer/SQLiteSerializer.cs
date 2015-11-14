using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using System.IO;
using System.Collections;

namespace SQLiteSerialization {
	public class SQLiteSerializer {
		public static string Version = "1.0.6b";
		public static string ArrayTableName = "arrays";
		public static string SerialInfoTableName = "serial_info";

		protected SQLiteConnection globalConn;
		protected StringBuilder sqlDefinitionRegion = new StringBuilder();		// sql here defines tables
        protected StringBuilder sqlDataRegion = new StringBuilder();            // sql here fills tables with data

		// Serialize vars
		protected int primaryKeyCount = 0;
		protected List<SerializedObjectTableRow> activeTables = new List<SerializedObjectTableRow>(0);		// list of tables in reverse order seen
		protected List<SerializedArray> activeArrays = new List<SerializedArray>(0);						// list of arrays in reverse order seen, stored in activeTables as primary source
		protected Dictionary<object,int> serializedObjects = new Dictionary<object,int>(0);					// list of objects with their UIDs in order seen

		// Deserialize vars
		protected Dictionary<long, SerialObjectsDefintion> serialTable;
		protected Dictionary<long, object> deserializedObjects = new Dictionary<long, object>(0);

		#region Main Serialization Functions
		// clean up all the vars from a previous call
		public void CleanUp() {
			try { closeSQLConnection(); } catch { }
			sqlDefinitionRegion = new StringBuilder();
			sqlDataRegion = new StringBuilder();
			primaryKeyCount = 0;
			activeTables = new List<SerializedObjectTableRow>(0);
			activeArrays = new List<SerializedArray>(0);
			serializedObjects = new Dictionary<object,int>(0);

			serialTable = new Dictionary<long, SerialObjectsDefintion>(0);
			deserializedObjects = new Dictionary<long, object>(0);
        }

		public void Serialize(object target, string databaseConnectionString, bool autoDeleteExistingFile=true) {
			if (File.Exists(databaseConnectionString.Replace("data source","").Replace("=","").Trim())) {   // TODO: this sucks, do something better later
				if (autoDeleteExistingFile)
					File.Delete(databaseConnectionString.Replace("data source", "").Replace("=", "").Trim());
				else
					throw new Exception("File already exists, cannot serialize over existing SQLite DB " + databaseConnectionString);
			}

			// build table objects in memory
			buildInfoTables();
			buildComplexObjectTable(target);        // if this is just a simple ValueType object, then I shall slap you with a mackrel

			// turn the table objects into SQL strings
			List<SQLiteParameter> bindParams = buildSQLStrings();

			// write it all out
			openSQLConnection(databaseConnectionString);
			if (isSQLiteReady()) {
				string c = sqlDefinitionRegion.Append(sqlDataRegion).ToString();
                SQLiteCommand cmd = new SQLiteCommand(c, globalConn);
				cmd.Parameters.AddRange(bindParams.ToArray());
                cmd.ExecuteNonQuery();
			}
			closeSQLConnection();
			CleanUp();
		}

		public T Deserialize<T>(string databaseConnectionString, bool ignoreMissing = false) {
			// connect and read the SQL data
			openSQLConnection(databaseConnectionString);
			if (!isSQLiteReady()) {
				throw new Exception("The SQLite database could not be made ready for reading.");
			}
			serialTable = readSerialSQLTable();
			T container = readSQLTableToObject<T>();

			closeSQLConnection();
			CleanUp();
			return container;
		}
		#endregion

		#region String operations
		protected List<SQLiteParameter> buildSQLStrings() {
			List<SQLiteParameter> bindParams = new List<SQLiteParameter>();
			List<string> createdTables = new List<string>(activeTables.Count);
			sqlDefinitionRegion.AppendFormat("CREATE TABLE {1}(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE,location TEXT,typename TEXT);{0}{0}",Environment.NewLine, SerialInfoTableName);

			foreach (SerializedObjectTableRow table in activeTables) {
				StringBuilder insertColDef = new StringBuilder();
				StringBuilder insertColValue = new StringBuilder();
				StringBuilder tableDefFK = new StringBuilder();
				bool doTableCreate = !createdTables.Contains(table.TableNameSQL);
				bool isArrayTable = (table.TableName == ArrayTableName);

				if (doTableCreate) {
					createdTables.Add(table.TableNameSQL);
					if (isArrayTable) {
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE,type INTEGER,typename TEXT,key_type TEXT,value_type TEXT);{1}", ArrayTableName,Environment.NewLine);
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}_entries(PK INTEGER PRIMARY KEY,UID INTEGER,__key__ TEXT,__value__ TEXT,FOREIGN KEY(UID) REFERENCES {0}(UID));{1}{1}", ArrayTableName,Environment.NewLine);
					} else {
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}", table.TableNameSQL);
						sqlDefinitionRegion.Append("(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE");
					}
				}
				
				if (isArrayTable) {
					int UIDParamNo = -1;
					SerializedArray sarr = findArray(table.UniqueID);
					sqlDataRegion.AppendFormat("INSERT INTO {4}(UID,location,typename) VALUES ({0},'{1}','{2}');{3}{3}",new object[] { sarr.UniqueID, ArrayTableName, table.ObjectType, Environment.NewLine, SerialInfoTableName});
					sqlDataRegion.AppendFormat("INSERT INTO {0}(UID,type,typename,key_type,value_type) VALUES (@v{1},@v{2},@v{3},@v{4},@v{5});{6}{6}",
												new object[] { ArrayTableName,bindParams.Count,bindParams.Count+1,bindParams.Count+2,bindParams.Count+3,bindParams.Count+4,Environment.NewLine });
					UIDParamNo = bindParams.Count;
                    bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.UniqueID));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, (int)sarr.ArrayType));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.TypeName));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.KeyType.AssemblyQualifiedName));   // .FullName
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.ValueType.AssemblyQualifiedName));
					insertColDef.AppendFormat("INSERT INTO {0}_entries(UID,__key__,__value__", ArrayTableName);

					bool firsttime = true;
					foreach (SerializedArrayItem item in sarr.Items) {
						if (!firsttime) insertColValue.Append("),"); firsttime = false;
						insertColValue.AppendFormat("(@v{0},@v{1},@v{2}", UIDParamNo, bindParams.Count,bindParams.Count + 1);
						bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, item.key));
						if (!SerializeUtilities.IsSimpleValue(sarr.ValueType) && (int)item.value == -1)
							bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, null));		// lol, be careful with this nulling when -1 thing
						else
							bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, item.value));
					}
                } else {
					sqlDataRegion.AppendFormat("INSERT INTO {4}(UID,location,typename) VALUES ({0},'{1}','{2}');{3}{3}", new object[] { table.UniqueID, table.TableNameSQL, table.ObjectType, Environment.NewLine, SerialInfoTableName });
					sqlDataRegion.AppendFormat("INSERT INTO {0}", table.TableNameSQL);
					insertColDef.Append("(UID");
					insertColValue.Append("(@v" + bindParams.Count);
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, table.UniqueID));

					foreach (SerializedObjectColumn col in table.Columns) {
						if (doTableCreate) {
							sqlDefinitionRegion.AppendFormat(",{0} {1}", col.sqlName, col.sqlType);
						}

						insertColDef.AppendFormat(",{0}", col.sqlName);
						insertColValue.AppendFormat(",@v{0}", bindParams.Count);
						if (col.sqlShortName.StartsWith("fk_") && (int)col.columnValue == -1)
							bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, null));
						else
							bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, col.columnValue));

						// is this a FK linkable?
						if (doTableCreate && col.sqlName.IndexOf("fk_") == 0) {
							tableDefFK.AppendFormat(",FOREIGN KEY({0}) REFERENCES {1}(UID)", col.sqlName, col.columnTypeSQLSafe);
						}
					}
				}

				if (doTableCreate && !isArrayTable) sqlDefinitionRegion.AppendFormat("{0});{1}{1}", tableDefFK, Environment.NewLine);
				sqlDataRegion.AppendFormat("{0}) VALUES {1});{2}{2}",insertColDef,insertColValue, Environment.NewLine);
            }

			//sqlDataRegion.Append("VACUUM");		// not sure if this is really needed when we are only doing one write
			return bindParams;
		}
		#endregion

		#region Internal Serialization Functions
		protected void buildInfoTables() {
		}

		protected T readSQLTableToObject<T>(int UID=1) {
			string tablename = serialTable[UID].tablename;
			Type localType = serialTable[UID].storageType;	 //typeof(T); - bug, using the type of the definition broke inherited objects

			if (tablename == ArrayTableName) {
				return readAllArrayEntries<T>(UID);
			} else {
				object container = SerializeUtilities.CreateUninitializedObject(localType);

				SerializedObjectTableRow table = readOneTableEntry(UID, tablename);
				if (SerializeUtilities.IsSimpleValue(localType)) {
					object val = table.Columns.Find(x => x.columnName.EndsWith("__value__")).columnValue;
                    container = (T)Convert.ChangeType(val, localType);
                } else {
					FieldInfo[] fields = SerializeUtilities.GetObjectFields(localType);
                    foreach (FieldInfo field in fields) {
						SerializedObjectColumn col = table.Columns.Find(x => x.columnName.Equals( SerializeUtilities.MakeSafeColumn(field.Name) ));
						if (col == null) continue;
                        object val = col.columnValue;
						Type castableType = field.FieldType;
						if (!SerializeUtilities.IsSimpleValue(field.FieldType) && val != null) {
							if (serialTable.ContainsKey((long)col.columnValue)) { castableType = serialTable[(long)col.columnValue].storageType ?? field.FieldType; }
							
							if (!deserializedObjects.ContainsKey((long)col.columnValue)) {
								val = SerializeUtilities.CallGenericMethodWithReflection(
									this, "readSQLTableToObject",
									new Type[] { field.FieldType },
									new object[] { Convert.ChangeType(val, typeof(int)) }
								);
								deserializedObjects.Add((long)col.columnValue, val);
							} else { val = deserializedObjects[(long)col.columnValue]; }
                        }
						//localType.InvokeMember("next", BindingFlags.SetField | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, container, new object[] { val });
						if (field.FieldType.IsEnum)
							field.SetValue(container, Enum.ToObject(field.FieldType, val));
						else
							field.SetValue(container, Convert.ChangeType(val, castableType));
					}
				}

				return (T)container;
			}
		}

		protected T readAllArrayEntries<T>(int UID) {
			T container = default(T);

			ArrayStorageDefinition definition = readArrayTableDefinition(UID);
			switch (definition.type) {
				case LinearObjectType.SystemArray:
					{
						Type arrayType = typeof(T);
                        Type containerType = arrayType.GetElementType();
						if (arrayType.GetArrayRank() == 1) {
							List<string> items = new List<string>();
							Dictionary<int, string> entries = readStdArrayTableEntries(UID);
							foreach (KeyValuePair<int, string> pair in entries) {
								items.Add(pair.Value);
							}
							Array completeArr = (Array)SerializeUtilities.CreateUninitializedObject(typeof(T), items.Count);
							for (int i = 0; i < items.Count; i++) {
								if (SerializeUtilities.IsSimpleValue(containerType)) {
									if (containerType.IsEnum)
										completeArr.SetValue(Enum.ToObject(containerType, items[i]), i);
									else
										completeArr.SetValue(castRawSQLiteArrayVal(items[i], containerType), i);
								} else {
									object o = SerializeUtilities.CallGenericMethodWithReflection(
										this, "readSQLTableToObject",
										new Type[] { containerType },
										new object[] { castRawSQLiteArrayVal(items[i], containerType) }
									);
									completeArr.SetValue(o, i);
								}
							}
							container = (T)Convert.ChangeType(completeArr, typeof(T));
						} else {
							Dictionary<string, string> entries = readMultiDimensionalArrayTableEntries(UID);
							List<long> dimensionRanges = new List<long>();

							int dims = 0;
							do {
								var result = from element in entries.Keys
											 orderby element ascending
											 where element.ToString().StartsWith(dims + "x")
											 select element;
								long c = result.Count();
								if (c > 0)
									dimensionRanges.Add(result.Count());
								dims++;
                            } while (dims == dimensionRanges.Count);
							dims = dimensionRanges.Count;

							Array completeArr = (Array)SerializeUtilities.CreateUninitializedObject(typeof(T), dimensionRanges.ToArray());

							long[] indicies = new long[dims];
							foreach (KeyValuePair<string,string> entry in entries) {
								// build in the index
								string[] preIndex = entry.Key.ToString().Split('x');
								for (long i=0; i < preIndex.LongLength; i++) {
									indicies[i] = long.Parse(preIndex[i]);
								}

								// pull the value
								object val;
								if (SerializeUtilities.IsSimpleValue(containerType)) {
									if (containerType.IsEnum)
										val = Enum.ToObject(containerType, entry.Value);
									else
										val = castRawSQLiteArrayVal(entry.Value, containerType);
								} else {
									val = SerializeUtilities.CallGenericMethodWithReflection(
										this, "readSQLTableToObject",
										new Type[] { containerType },
										new object[] { castRawSQLiteArrayVal(entry.Value, containerType) }
									);
								}

								// slap it with a fish
								completeArr.SetValue(val, indicies);
							}
							container = (T)Convert.ChangeType(completeArr, typeof(T));
						}
					}
					break;

				case LinearObjectType.IEnumerableFamily:
					{
						List<string> items = new List<string>();
						Type containerType = definition.valueTypeName;

						Dictionary<int, string> entries = readStdArrayTableEntries(UID);
						foreach (KeyValuePair<int, string> pair in entries) {
							items.Add(pair.Value);
						}
						object completeArr = Activator.CreateInstance(typeof(T));
						for (int i = 0; i < items.Count; i++) {
							if (SerializeUtilities.IsSimpleValue(containerType)) {
								if (containerType.IsEnum)
									((IList)completeArr).Insert(i, Enum.ToObject(containerType, items[i]));
								else
									((IList)completeArr).Insert(i, castRawSQLiteArrayVal(items[i], containerType));
							} else {
								object o = SerializeUtilities.CallGenericMethodWithReflection(
									this, "readSQLTableToObject",
									new Type[] { containerType },
									new object[] { castRawSQLiteArrayVal(items[i], containerType) }
								);
								((IList)completeArr).Insert(i, o);
							}
						}
						container = (T)completeArr;
					}
					break;

				case LinearObjectType.IDictionaryFamily:
					{
						object completeArr = Activator.CreateInstance(typeof(T));

						Type keyType = definition.keyTypeName;
						Type containerType = definition.valueTypeName;

						Dictionary<string, string> entries = readComplexArrayTableEntries(UID);
						foreach (KeyValuePair<string, string> pair in entries) {
							object keyHolder;
							if (SerializeUtilities.IsSimpleValue(keyType)) {
								if (keyType.IsEnum)
									keyHolder = Enum.ToObject(keyType, pair.Key);
								else
									keyHolder = castRawSQLiteArrayVal(pair.Key, keyType);
							} else {
								keyHolder = SerializeUtilities.CallGenericMethodWithReflection(
									this, "readSQLTableToObject",
									new Type[] { keyType },
									new object[] { castRawSQLiteArrayVal(pair.Key, keyType) }
								);
							}
							object valueHolder;
							if (SerializeUtilities.IsSimpleValue(containerType)) {
								if (containerType.IsEnum)
									valueHolder = Enum.ToObject(containerType, pair.Value);
								else
									valueHolder = castRawSQLiteArrayVal(pair.Value, containerType);
							} else {
								valueHolder = SerializeUtilities.CallGenericMethodWithReflection(
									this, "readSQLTableToObject",
									new Type[] { containerType },
									new object[] { castRawSQLiteArrayVal(pair.Value, containerType) }
								);
							}
							((IDictionary)completeArr).Add(keyHolder,valueHolder);
						}
						container = (T)completeArr;
					}
					break;
			}

			return container;
		}

		protected int buildComplexObjectTable(object target) {
			if (target == null) return -1;

			// check and add object to the global seen list. Makes unique objects (top-down) and stops infinite recursion
			if (hasBeenSeenBefore(target))
				return serializedObjects[target];     // return the already proc'ed PK

			Type localType = target.GetType();
			// interestingly, my serializer doesn't require [Serializable] because it is pure reflection. Serializers probably use FormatterServices.
			//if (!canSerialize(target))
			//	throw new Exception("Your object is not serializable! Please add [Serializable] to the class definition for " + localType.Name + " and all child objects you are attempting to store.");

			// can't handle this because it is doomed to become abstract
			if (SerializeUtilities.IsArrayLike(localType))
				return buildArrayTable(target);

			int localPK = ++primaryKeyCount;
			serializedObjects.Add(target, localPK);
			
			SerializedObjectTableRow table = new SerializedObjectTableRow(localPK,localType);

			if (SerializeUtilities.IsSimpleValue(localType)) { // if you passed in a simple value, we need to handle table construction
				table.AddColumn(new SerializedObjectColumn("__value__", localType.FullName, target));
			} else {
				FieldInfo[] fields = SerializeUtilities.GetObjectFields(localType);

				foreach (FieldInfo field in fields) {
					Type fieldType = field.FieldType;
					serializeSubObject(table, fieldType, field, target);
				}
			}

			activeTables.Add(table);
			return localPK;
		}

		protected void serializeSubObject(SerializedObjectTableRow table, Type fieldType, FieldInfo field, object parentObj) {
			// Condition 1: Simple Val
			if (SerializeUtilities.IsSimpleValue(fieldType)) {
				// we can store this raw
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, field.GetValue(parentObj));
				table.AddColumn(col);
			} else if (SerializeUtilities.IsArrayLike(field.FieldType)) {
				// Condition 2: Dictionaries
				// Condition 3: Handle Enumerable collections like List<>, but not core system arrays
				// Condition 4: System Arrays

				// make an array-table entry
				int FK = buildArrayTable(field.GetValue(parentObj));
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, ArrayTableName, FK);
				table.AddColumn(col);

				// Condition 5: Complex Sub-Component
			} else {
				// this is a complex type (a class or something), so recurse
				int FK = buildComplexObjectTable(field.GetValue(parentObj));   // we need a Foreign Key (otherwise objects would randomize themselves across the object structure on each load [goofy effect])

				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, FK);
				table.AddColumn(col);
			}
		}

		protected int buildArrayTable(object target) {
			if (target == null) return -1;

			// check and add object to the global seen list. Makes unique objects (top-down) and stops infinite recursion
			if (hasBeenSeenBefore(target))
				return serializedObjects[target];     // return the already proc'ed PK

			int localPK = ++primaryKeyCount;
			serializedObjects.Add(target, localPK);
			Type localType = target.GetType();

			// put it in the dict list with its PK
			SerializedObjectTableRow arrayTable = new SerializedObjectTableRow(localPK, ArrayTableName);
			SerializedArray arrayDef = new SerializedArray(localPK, localType);

			// for each based on special handling
			switch (arrayDef.ArrayType) {
				case LinearObjectType.SystemArray:
					Array arrHolder = (Array)target;
					int arrDimensions = arrHolder.Rank;
					if (arrDimensions == 1) {
						for (int index = arrHolder.GetLowerBound(0); index <= arrHolder.GetUpperBound(0); index++) {
							object val = arrHolder.GetValue(index);
							if (val != null) {      // arrays should never have non-contiguous values, so nulls are just the pre-alloced storage space
								if (SerializeUtilities.IsSimpleValue(localType.GetElementType())) {
									arrayDef.AddValues((long)index, val);
								} else {
									arrayDef.AddValues((long)index, buildComplexObjectTable(val));
								}
							}
						}
					} else {    // the internet was useless for generically looping over a multidimensional array (so, if it is wrong, it's all me)
						// setup the ranges of dimensional indexability
						KeyValuePair<long, long>[] dimensionRanges = new KeyValuePair<long, long>[arrDimensions];
						long[] indicies = new long[arrDimensions];
						indicies.Initialize();      // zeroes out the indicies
						for (int dimension = 0; dimension < arrDimensions; dimension++) {
							dimensionRanges[dimension] = new KeyValuePair<long, long>(arrHolder.GetLowerBound(dimension), arrHolder.GetUpperBound(dimension));
							indicies[dimension] = arrHolder.GetLowerBound(dimension);
						}

						bool workinIt = true;
						do {
							// turns out do-while loops are useful for truely fucked up shit
							object val = arrHolder.GetValue(indicies);
							if (val != null) {
								if (SerializeUtilities.IsSimpleValue(localType.GetElementType())) {
									arrayDef.AddValues(string.Join<long>("x",indicies),val);
								} else {
									arrayDef.AddValues(string.Join<long>("x", indicies), buildComplexObjectTable(val));
								}
							}

							// take a step...
							try {
								int dimInc = arrDimensions - 1;
								for (;;) {
									indicies[dimInc]++;
									if (indicies[dimInc] > dimensionRanges[dimInc].Value) {    // greater than upper bound?
										indicies[dimInc] = dimensionRanges[dimInc].Key;        // ...then set it back to lower and move up
										dimInc--;
									} else { break; }
									if (dimInc < 0) { workinIt = false; break; }
								}
							} catch { workinIt = false; }
						} while (workinIt);
					}
					break;

				case LinearObjectType.IEnumerableFamily:
					IEnumerable<object> enumHolder = (IEnumerable<object>)target;
					uint indexCnt = 0;
					foreach (object item in enumHolder) {
						if (SerializeUtilities.IsSimpleValue(item.GetType())) {
							arrayDef.AddValues(indexCnt, item);
						} else {
							arrayDef.AddValues(indexCnt, buildComplexObjectTable(item));
						}
						indexCnt++;
                    }
					break;

				case LinearObjectType.IDictionaryFamily:
					IDictionary dictHolder = (IDictionary)target;
					foreach (object key in dictHolder.Keys) {
						object processedKey = key;
						object processedValue = dictHolder[key];

						// process the key type
						if (!SerializeUtilities.IsSimpleValue(key.GetType())) {
							processedKey = buildComplexObjectTable(key);
						}
						// process the value type
						if (!SerializeUtilities.IsSimpleValue(processedValue.GetType())) {
							processedValue = buildComplexObjectTable(processedValue);
						}

						arrayDef.AddValues(processedKey, processedValue);
					}
					break;

				default:
					throw new Exception("You fucked up. Go back. Not a recognized array-like object, try serializing as a complex.");
					//break;
			}

			// save it and return the Primary Key as a Foreign Key
			activeArrays.Add(arrayDef);
			activeTables.Add(arrayTable);
			return localPK;
		}
		#endregion

		#region Utility Functions
		protected SerializedArray findArray(int uniqueID) {
			foreach (SerializedArray a in activeArrays) {
				if (a.UniqueID == uniqueID) {
					return a;
				}
			}

			return null;
		}

		public bool canSerialize(object target) {
			Type targetType = target.GetType();
			return Attribute.IsDefined(targetType, typeof(SerializableAttribute)) || targetType.IsSerializable || (target is ISerializable);
		}

		protected bool hasBeenSeenBefore(object targetCheck) {
			return serializedObjects.ContainsKey(targetCheck);
		}
		#endregion

		#region SQLite specific functions
		public SQLiteConnection getRawConnection() { return globalConn; }

		protected bool isSQLiteReady() {
			return globalConn.State == ConnectionState.Open || globalConn.State == ConnectionState.Executing || globalConn.State == ConnectionState.Fetching;
		}

		// databaseConnectionString = can actually just be a file path, but optional pass complex connection strings
		protected void openSQLConnection(string connectionString) {
            globalConn = new SQLiteConnection(formatConnectionString(connectionString));
			globalConn.Open();
		}

		protected string formatConnectionString(string connectionString) {
			if (connectionString.IndexOf("URI") < 0)
				connectionString = "data source = " + connectionString;
            return connectionString;
        }

		protected void closeSQLConnection() {
			globalConn.Close();
		}

		protected Dictionary<long, SerialObjectsDefintion> readSerialSQLTable() {
			Dictionary<long, SerialObjectsDefintion> serialTable = new Dictionary<long, SerialObjectsDefintion>();
            string sql = string.Format("SELECT UID,location,typename FROM {0} ORDER BY UID", SerialInfoTableName);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();

			while (dr.Read()) {
				serialTable.Add(Convert.ToInt64(dr["UID"]),
								new SerialObjectsDefintion(
									Convert.ToInt32(dr["UID"]),
									Convert.ToString(dr["location"]),
									Convert.ToString(dr["typename"])
								)
							);
			}

			return serialTable;
		}

		protected object castRawSQLiteArrayVal(string rawValue,Type targetType) {
			if (SerializeUtilities.IsSimpleValue(targetType)) {
				switch (targetType.FullName.ToLower().Trim().Replace("system.", "")) {
					case "single":
					case "float":
						return float.Parse(rawValue);
					case "decimal":
						return decimal.Parse(rawValue);
					case "double":
						return double.Parse(rawValue);
					case "string":
						return rawValue;
					case "char":
						return (char)int.Parse(rawValue);       // I don't know why SQLite insists on storing the value like this, probably my code...
					case "int16":
					case "short":
						return short.Parse(rawValue);
					case "uint16":
					case "ushort":
						return ushort.Parse(rawValue);
					case "int32":
					case "int":
						return int.Parse(rawValue);
					case "uint32":
					case "uint":
						return uint.Parse(rawValue);
					case "int64":
					case "long":
						return long.Parse(rawValue);
					case "uint64":
					case "ulong":
						return ulong.Parse(rawValue);
					case "byte":
						return byte.Parse(rawValue);
					case "sbyte":
						return sbyte.Parse(rawValue);
					case "bool":
					case "boolean":
						return bool.Parse(rawValue);
					case "datetime":
					case "date":
					case "time":
						return DateTime.Parse(rawValue);        // not sure what is going to happen here, catch it in a unit test
					default:
						return Convert.ChangeType(rawValue, targetType);
						//return long.Parse(rawValue);             // this.... should never happen and something bad caused this OR it's an enum?
				}
			} else { return Convert.ChangeType(rawValue, typeof(int)); }
		}

		protected ArrayStorageDefinition readArrayTableDefinition(int UID) {
			string sql = string.Format("SELECT type,typename,key_type,value_type FROM {0} WHERE UID = {1} LIMIT 1", ArrayTableName, UID);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();

			dr.Read();
			ArrayStorageDefinition def = new ArrayStorageDefinition();
			def.type = (LinearObjectType)Convert.ToInt32(dr["type"]);
            def.linearObjectTypeName = Type.GetType(dr["typename"].ToString());
			def.keyTypeName = Type.GetType(dr["key_type"].ToString());
			def.valueTypeName = Type.GetType(dr["value_type"].ToString());

			return def;
		}

        protected Dictionary<int, string> readStdArrayTableEntries(int UID) {
			Dictionary<int, string> arrayEntries = new Dictionary<int, string>();
			string sql = string.Format("SELECT __key__,__value__ FROM {0}_entries WHERE UID = {1} ORDER BY PK", ArrayTableName, UID);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();

			while (dr.Read()) {
				int key = Convert.ToInt32(dr["__key__"]);
				string val;
				if (dr.IsDBNull(dr.GetOrdinal("__value__")))
					val = null;
				else
					val = dr["__value__"].ToString();
				arrayEntries.Add(key,val);
			}

			return arrayEntries;
		}

		protected Dictionary<string,string> readMultiDimensionalArrayTableEntries(int UID) {
			Dictionary<string, string> arrayEntries = new Dictionary<string, string>();
			string sql = string.Format("SELECT __key__,__value__ FROM {0}_entries WHERE UID = {1} ORDER BY PK", ArrayTableName, UID);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();

			while (dr.Read()) {
				string key = dr["__key__"].ToString();
				string val;
				if (dr.IsDBNull(dr.GetOrdinal("__value__")))
					val = null;
				else
					val = dr["__value__"].ToString();
				arrayEntries.Add(key, val);
			}

			return arrayEntries;
		}

		protected Dictionary<string,string> readComplexArrayTableEntries(int UID) {
			Dictionary<string, string> arrayEntries = new Dictionary<string, string>();
			string sql = string.Format("SELECT __key__,__value__ FROM {0}_entries WHERE UID = {1} ORDER BY PK", ArrayTableName, UID);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();

			while (dr.Read()) {
				string key = dr["__key__"].ToString();		// if you have null keys, I'm personally going to hunt you down and make you watch old Olsen twin movies
				string val;
				if (dr.IsDBNull(dr.GetOrdinal("__value__")))
					val = null;
				else
					val = dr["__value__"].ToString();
				arrayEntries.Add(key, val);
			}

			return arrayEntries;
		}

		protected SerializedObjectTableRow readOneTableEntry(int UID,string tablename) {
			SerializedObjectTableRow entry = new SerializedObjectTableRow(UID, tablename);
			Regex colTypeStripper = new Regex("^(f_|d_|s_|c_|i_|b_|dt_|fk_)");

			string sql = string.Format("SELECT * FROM {0} WHERE UID={1} LIMIT 1", tablename, UID);
			SQLiteCommand cmd = new SQLiteCommand(sql, globalConn);
			SQLiteDataReader dr = cmd.ExecuteReader();
			while (dr.Read()) {
				for (int ci=0; ci<dr.FieldCount; ci++) {
					string colname = dr.GetName(ci);
					string coltype = colname.Split('_')[0];
					object colval;
					if (dr.IsDBNull(ci))
						colval = null;
					else
						colval = dr.GetValue(ci);

					colname = colTypeStripper.Replace(colname,"");
                    entry.AddColumn(new SerializedObjectColumn(colname,coltype,colval));
                }
			}
			return entry;

			// TODO: Make this faster by using the below code to read entire table the first time requested and cache it. Then, return each row on each call
			/***
			 SqlConnection conn = new SqlConnection("connectionstringhere");
            SqlCommand cmd = new SqlCommand("SELECT * FROM TABLE", conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            foreach (DataRow dr in dt.Rows)
            {
                MyFunction(dr["Id"].ToString(), dr["Name"].ToString());
            }
			***/
		}
		#endregion
	}
}
