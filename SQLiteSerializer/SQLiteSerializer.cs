using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SQLiteSerializer {
	// TODO: Currently has limitations with processing types that have Generic Arguments with Parameter Constraints
	// TODO: Pure reference values (simple values that were passed by reference) are duplicated when they are deserialized
	public class SQLiteSerializer {
		protected SQLiteConnection globalConn;
		protected StringBuilder sqlDefinitionRegion = new StringBuilder();		// sql here defines tables
        protected StringBuilder sqlDataRegion = new StringBuilder();            // sql here fills tables with data

		protected int primaryKeyCount = 0;
		protected List<SerializedObjectTable> activeTables = new List<SerializedObjectTable>();
		protected List<SerializedArray> activeArrays = new List<SerializedArray>();		// TODO: Merge these two, shouldn't be a need to store separate
		protected Dictionary<object,int> processedComplexObjects = new Dictionary<object, int>();

		#region Main Serialization Functions
		public void Serialize(object target, string databaseConnectionString) {
			// build table objects in memory
			buildInfoTables();
			buildComplexObjectTable(target);        // if this is just a simple ValueType object, then I shall slap you with a mackrel

			// turn the table objects into SQL strings
			List<SQLiteParameter> bindParams = buildSQLStrings();

			// write it all out
			openSQLConnection(databaseConnectionString);
			if (isSQLiteReady()) {
				SQLiteCommand cmd = new SQLiteCommand(sqlDefinitionRegion.Append(sqlDataRegion).ToString(), globalConn);
				cmd.ExecuteNonQuery();
			}
			closeSQLConnection();
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

			return container;
		}
		#endregion

		#region String operations
		protected List<SQLiteParameter> buildSQLStrings() {
			List<SQLiteParameter> bindParams = new List<SQLiteParameter>();
			//protected StringBuilder sqlDefinitionRegion = new StringBuilder();      // sql here defines tables
			//protected StringBuilder sqlDataRegion = new StringBuilder();            // sql here fills tables with data
			//protected List<SerializedObjectTable> activeTables = new List<SerializedObjectTable>();
			//protected List<SerializedArray> activeArrays = new List<SerializedArray>();
			foreach (SerializedObjectTable table in activeTables) {
				StringBuilder insertColDef = new StringBuilder();
				StringBuilder insertColValue = new StringBuilder();

				sqlDefinitionRegion.AppendFormat("CREATE TABLE {0}",table.TableName);
				sqlDefinitionRegion.Append("(PK INTEGER PRIMARY KEY AUTOINCREMENT,UID INTEGER UNIQUE");
				sqlDataRegion.AppendFormat("INSERT INTO {0}",table.TableName);
				insertColDef.Append("(UID)");
				insertColValue.Append("(@v" + bindParams.Count);
				bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, table.UniqueID));

				foreach (SerializedObjectColumn col in table.Columns) {
					sqlDefinitionRegion.AppendFormat(",{0} {1}",col.sqlName, col.sqlType);

					insertColDef.AppendFormat(",@v{0}",bindParams.Count);
					bindParams.Add(new SQLiteParameter("@v" + bindParams.Count, col.columnValue));
                }

				sqlDefinitionRegion.AppendFormat(");{0}", Environment.NewLine);
				sqlDataRegion.AppendFormat("{0}) VALUES {1});{2}",insertColDef,insertColValue, Environment.NewLine + Environment.NewLine);
            }

			sqlDataRegion.Append("VACUUM");		// not sure if this is really needed when we are only doing one write
			return bindParams;
		}
		#endregion

		#region Internal Serialization Functions
		protected bool hasBeenSeenBefore(object targetCheck) {
			return processedComplexObjects.ContainsKey(targetCheck);
		}

		protected bool IsSimpleValue(Type type) {
			return (type.IsPrimitive || type.IsEnum || type.IsValueType || type.Equals(typeof(string)) || type.IsSubclassOf(typeof(ValueType)));
		}

		protected void buildInfoTables() {
		}

		protected int buildComplexObjectTable(object target) {
			// check and add object to the global seen list. Makes unique objects (top-down) and stops infinite recursion
			if (hasBeenSeenBefore(target))
				return processedComplexObjects[target];		// return the already proc'ed PK

			int localPK = ++primaryKeyCount;
			processedComplexObjects.Add(target, localPK);

			Type localType = target.GetType();
			if (!canSerialize(target))
				throw new Exception("Your object is not serializable! Please add [Serializable] to the class definition for " + localType.Name + " and all child objects you are attempting to store.");

			//localType.Module	- TODO: add this to info tables
			//localType.AssemblyQualifiedName	- TODO: add this to info tables
			SerializedObjectTable table = new SerializedObjectTable(localPK,localType.FullName);

			FieldInfo[] fields = localType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields) {
				Type fieldType = field.FieldType;
				serializeSubObject(table,fieldType,field,target);
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
			if (hasBeenSeenBefore(target))
				return processedComplexObjects[target];     // return the already proc'ed PK

			int localPK = ++primaryKeyCount;
			processedComplexObjects.Add(target, localPK);
			Type localType = target.GetType();

			// put it in the dict list with its PK
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

			// save it and return the Primer as a Foreign Key
			activeArrays.Add(arrayDef);
			return localPK;
		}

		public bool canSerialize(object target) {
			Type targetType = target.GetType();
			return Attribute.IsDefined(targetType, typeof(SerializableAttribute)) || targetType.IsSerializable || (target is ISerializable);
        }
		#endregion

		#region SQLite specific functions
		public SQLiteConnection getRawConnection() { return globalConn; }

		protected bool isSQLiteReady() {
			return globalConn.State == ConnectionState.Open || globalConn.State == ConnectionState.Executing || globalConn.State == ConnectionState.Fetching;
		}

		// databaseConnectionString = can actually just be a file path, but optional pass complex connection strings
		protected void openSQLConnection(string databaseConnectionString) {
			if (databaseConnectionString.IndexOf("URI") < 0)
				databaseConnectionString = "URI=file:" + databaseConnectionString;
            globalConn = new SQLiteConnection(databaseConnectionString);
			globalConn.Open();
		}

		protected void closeSQLConnection() {
			globalConn.Close();
		}
		#endregion
	}
}
