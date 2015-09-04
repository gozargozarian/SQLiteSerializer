using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SQLiteSerializer {
	public class SerializedObjectColumn {
		public string columnName { get; set; }
		public string columnType { get; set; }
		public object columnValue { get; set; }
		public SerializedObjectColumn(string columnName, string columnType, object columnValue) {
			this.columnName = columnName;
			this.columnType = columnType;
			this.columnValue = columnValue;
		}
	}

	public class SerializedObjectTable {
		string tablename;
		List<SerializedObjectColumn> columns;

		public SerializedObjectTable(string tablename) {
			this.tablename = tablename;
			columns = new List<SerializedObjectColumn>();
		}

		public void AddColumn(SerializedObjectColumn colDef) {
			columns.Add(colDef);
        }
	}

	public class SQLiteSerializer {
		protected SQLiteConnection globalConn;
		protected StringBuilder sqlDefinitionRegion = new StringBuilder();		// sql here defines tables
        protected StringBuilder sqlDataRegion = new StringBuilder();            // sql here fills tables with data

		protected int primaryKeyCount = 0;
		protected List<SerializedObjectTable> activeTables = new List<SerializedObjectTable>();
		protected Dictionary<object,int> processedComplexObjects = new Dictionary<object, int>();

		public void Serialize(object target, string databaseConnectionString) {
			buildInfoTables();
			buildComplexObjectTables(target);		// if this is just a simple ValueType object, then I shall slap you with a mackrel

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

			return container;
		}

		protected bool hasBeenSeenBefore(object targetCheck) {
			return processedComplexObjects.ContainsKey(targetCheck);
        }

		#region Internal Serialization Functions
		protected void buildInfoTables() {
		}

		protected int buildComplexObjectTables(object target) {
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
			SerializedObjectTable table = new SerializedObjectTable(localType.FullName);

			FieldInfo[] fields = localType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields) {
				Type fieldType = field.FieldType;
				if (fieldType.IsPrimitive || fieldType.IsEnum || fieldType.IsValueType || fieldType.Equals(typeof(string)) || fieldType.IsSubclassOf(typeof(ValueType))) {
					// we can store this raw
					SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, field.GetValue(target));
					table.AddColumn(col);
				} else if (field.FieldType.GetInterface(typeof(IDictionary<,>).FullName) != null) {		// handle dicts
					// make a dict entry
					// foreach the objs
				} else if (field.FieldType.GetInterface(typeof(IEnumerable<>).FullName) != null) {		// handle all other collections
					// make an array entry
					// foreach the objs
				} else {
					// this is a complex type (a class or something), so recurse
					int FK = buildComplexObjectTables(field.GetValue(target));	// we need a Foreign Key (otherwise objects would randomize themselves across the object structure on each load [goofy effect])

                    SerializedObjectColumn col = new SerializedObjectColumn(field.Name, fieldType.FullName, FK);
					table.AddColumn(col);
				}
			}

			activeTables.Add(table);
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
