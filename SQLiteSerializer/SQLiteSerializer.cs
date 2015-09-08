using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using System.IO;

namespace SQLiteSerialization {
	// TODO: Currently has limitations with processing types that have Generic Arguments with Parameter Constraints
	// TODO: Pure reference values (simple values that were passed by reference) are duplicated when they are deserialized
	public class SQLiteSerializer {
		public static string ArrayTableName = "arrays";

		protected SQLiteConnection globalConn;
		protected StringBuilder sqlDefinitionRegion = new StringBuilder();		// sql here defines tables
        protected StringBuilder sqlDataRegion = new StringBuilder();            // sql here fills tables with data

		protected int primaryKeyCount = 0;
		protected List<SerializedObjectTable> activeTables = new List<SerializedObjectTable>();		// list of tables in reverse order seen
		protected List<SerializedArray> activeArrays = new List<SerializedArray>();					// list of arrays in reverse order seen, stored in activeTables as primary source
		protected Dictionary<object,int> processedComplexObjects = new Dictionary<object, int>();	// list of objects with their UIDs in order seen

		#region Main Serialization Functions
		// clean up all the vars from a previous call
		public void CleanUp() {
			try { closeSQLConnection(); } catch { }
			sqlDefinitionRegion = new StringBuilder();
			sqlDataRegion = new StringBuilder();
			primaryKeyCount = 0;
			activeTables = new List<SerializedObjectTable>();
			activeArrays = new List<SerializedArray>();
			processedComplexObjects = new Dictionary<object, int>();
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
			databaseConnectionString = formatConnectionString(databaseConnectionString);
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
			T container = default(T);

			/***
			// wake up generic types:
			// construct the generic type in order to walk it
			Type baseGeneric = typeof(IEnumerable<>);
			Type[] genericArgument = { arrayDef.ValueType };
			Type constructed = baseGeneric.MakeGenericType(genericArgument);
			var enumHolder = Activator.CreateInstance(constructed);
			***/

			CleanUp();
			return container;
		}
		#endregion

		#region String operations
		protected List<SQLiteParameter> buildSQLStrings() {
			List<SQLiteParameter> bindParams = new List<SQLiteParameter>();
			List<string> createdTables = new List<string>(activeTables.Count);

			foreach (SerializedObjectTable table in activeTables) {
				StringBuilder insertColDef = new StringBuilder();
				StringBuilder insertColValue = new StringBuilder();
				StringBuilder tableDefFK = new StringBuilder();
				bool doTableCreate = !createdTables.Contains(table.TableNameSQL);
				bool isArrayTable = (table.TableName == ArrayTableName);

				if (doTableCreate) {
					createdTables.Add(table.TableNameSQL);
					if (isArrayTable) {
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE,type INTEGER,typename TEXT,key_type TEXT,value_type TEXT);{1}", ArrayTableName,Environment.NewLine);
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}_entries(UID INTEGER,__key__ NUMERIC,__value__ NUMERIC,FOREIGN KEY(UID) REFERENCES {0}(UID));{1}{1}", ArrayTableName,Environment.NewLine);
					} else {
						sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}", table.TableNameSQL);
						sqlDefinitionRegion.Append("(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE");
					}
				}
				
				if (isArrayTable) {
					SerializedArray sarr = FindArray(table.UniqueID);
					sqlDataRegion.AppendFormat("INSERT INTO {0}(UID,type,typename,key_type,value_type) VALUES (@v{1},@v{2},@v{3},@v{4},@v{5});{6}{6}",
												new object[] { ArrayTableName,bindParams.Count,bindParams.Count+1,bindParams.Count+2,bindParams.Count+3,bindParams.Count+4,Environment.NewLine });
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.UniqueID));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, (int)sarr.ArrayType));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.TypeName));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.KeyType.FullName));
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, sarr.ValueType.FullName));
					insertColDef.AppendFormat("INSERT INTO {0}_entries(UID,__key__,__value__", ArrayTableName);

					bool firsttime = true;
					foreach (SerializedArrayItem item in sarr.Items) {
						if (!firsttime) { insertColValue.Append("),"); firsttime = false; }
						insertColValue.AppendFormat("(@v{0},@v{1}",bindParams.Count,bindParams.Count + 1);
						bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, item.key));
						bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, item.value));
					}
				} else {
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

			sqlDataRegion.Append("VACUUM");		// not sure if this is really needed when we are only doing one write
			return bindParams;
		}
		#endregion

		#region Internal Serialization Functions
		protected void buildInfoTables() {
		}

		protected int buildComplexObjectTable(object target) {
			// check and add object to the global seen list. Makes unique objects (top-down) and stops infinite recursion
			if (HasBeenSeenBefore(target))
				return processedComplexObjects[target];		// return the already proc'ed PK

			int localPK = ++primaryKeyCount;
			processedComplexObjects.Add(target, localPK);

			Type localType = target.GetType();
			if (!canSerialize(target))
				throw new Exception("Your object is not serializable! Please add [Serializable] to the class definition for " + localType.Name + " and all child objects you are attempting to store.");

			//localType.Module	- TODO: add this to info tables
			//localType.AssemblyQualifiedName	- TODO: add this to info tables
			SerializedObjectTable table = new SerializedObjectTable(localPK,localType.FullName);

			if (IsSimpleValue(localType)) { // if you passed in a simple value, we need to handle table construction
				table.AddColumn(new SerializedObjectColumn("__value__", localType.FullName, target));
			} else {
				FieldInfo[] fields = localType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				foreach (FieldInfo field in fields) {
					Type fieldType = field.FieldType;
					serializeSubObject(table, fieldType, field, target);
				}
			}

			activeTables.Add(table);
			return localPK;
		}

		protected void serializeSubObject(SerializedObjectTable table, Type fieldType, FieldInfo field, object parentObj) {
			// Condition 1: Simple Val
			if (IsSimpleValue(fieldType)) {
				// we can store this raw
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, field.GetValue(parentObj));
				table.AddColumn(col);

				// Condition 2: Dictionaries
			} else if (field.FieldType.GetInterface(typeof(IDictionary<,>).FullName) != null) {
				// make a dict entry
				int FK = buildArrayTable(field.GetValue(parentObj));
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, FK);
				table.AddColumn(col);

				// Condition 3: Handle Enumerable collections like List<>, but not core system arrays
			} else if (field.FieldType.GetInterface(typeof(IEnumerable<>).FullName) != null && !fieldType.IsArray) {
				// make an Enumerable entry
				int FK = buildArrayTable(field.GetValue(parentObj));
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, FK);
				table.AddColumn(col);

				// Condition 4: System Arrays
			} else if (fieldType.IsArray) {
				// make a base array entry
				int FK = buildArrayTable(field.GetValue(parentObj));
				SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, FK);
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
			// check and add object to the global seen list. Makes unique objects (top-down) and stops infinite recursion
			if (HasBeenSeenBefore(target))
				return processedComplexObjects[target];     // return the already proc'ed PK

			int localPK = ++primaryKeyCount;
			processedComplexObjects.Add(target, localPK);
			Type localType = target.GetType();

			// put it in the dict list with its PK
			SerializedObjectTable arrayTable = new SerializedObjectTable(localPK, ArrayTableName);
			SerializedArray arrayDef = new SerializedArray(localPK, localType);

			// for each based on special handling
			switch (arrayDef.ArrayType) {
				case LinearObjectType.SystemArray:
					Array arrHolder = (Array)target;
					for (uint index = 0; index < arrHolder.Length; index++) {
						object val = arrHolder.GetValue(index);
						if (IsSimpleValue(val.GetType())) {
							arrayDef.AddValues(index, val);
						} else {
							arrayDef.AddValues(index, buildComplexObjectTable(val));
						}
					}
					break;

				case LinearObjectType.IEnumerableFamily:
					IEnumerable<object> enumHolder = (IEnumerable<object>)target;
					uint indexCnt = 0;
					foreach (object item in enumHolder) {
						if (IsSimpleValue(item.GetType())) {
							arrayDef.AddValues(indexCnt, item);
						} else {
							arrayDef.AddValues(indexCnt, buildComplexObjectTable(item));
						}
						indexCnt++;
                    }
					break;

				case LinearObjectType.IDictionaryFamily:
					IDictionary<object,object> dictHolder = (IDictionary<object,object>)target;
					foreach (KeyValuePair<object,object> item in dictHolder) {
						object processedKey = item.Key;
						object processedValue = item.Value;
						// process the key type
						if (!IsSimpleValue(processedKey.GetType())) {
							processedKey = buildComplexObjectTable(processedKey);
						}
						// process the value type
						if (!IsSimpleValue(processedValue.GetType())) {
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
		protected SerializedArray FindArray(int uniqueID) {
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

		protected bool HasBeenSeenBefore(object targetCheck) {
			return processedComplexObjects.ContainsKey(targetCheck);
		}

		protected bool IsSimpleValue(Type type) {
			return (type.IsPrimitive || type.IsEnum || type.IsValueType || type.Equals(typeof(string)) || type.IsSubclassOf(typeof(ValueType)));
		}
		#endregion

		#region SQLite specific functions
		public SQLiteConnection getRawConnection() { return globalConn; }

		protected bool isSQLiteReady() {
			return globalConn.State == ConnectionState.Open || globalConn.State == ConnectionState.Executing || globalConn.State == ConnectionState.Fetching;
		}

		// databaseConnectionString = can actually just be a file path, but optional pass complex connection strings
		protected void openSQLConnection(string connectionString) {
            globalConn = new SQLiteConnection(connectionString);
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
		#endregion
	}
}
