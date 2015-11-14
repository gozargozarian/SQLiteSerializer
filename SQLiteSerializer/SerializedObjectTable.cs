using System;
using System.Collections.Generic;

namespace SQLiteSerialization {
	public class SerializedObjectColumn : IEquatable<SerializedObjectColumn> {
		public string columnName { get; set; }
		public object columnValue { get; set; }

		private string _columnType;
		private string _columnTypeSQLSafe;
		public string columnType {
			get { return _columnType; }
			set { _columnType = value; _columnTypeSQLSafe = SerializeUtilities.MakeSafeSQLType(value); }
		}
		public string columnTypeSQLSafe { get { return _columnTypeSQLSafe; } }

        public string sqlName {
			get {
				return sqlShortName + SerializeUtilities.MakeSafeColumn(columnName);
            }
        }
		public string sqlShortName {
			get {
				switch (columnType.ToLower().Trim().Replace("system.","")) {
					case "single":
					case "float":
						return "f_";
					case "decimal":
					case "double":
					case "quad":    // lol, for those 128-bit processors out there
						return "d_";
					case "string":
						return "s_";
					case "char":
						return "c_";
					case "int":
					case "uint":
					case "ulong":
					case "long":
					case "int2":
					case "int4":
					case "int8":
					case "int16":
					case "uint16":
					case "int32":
					case "uint32":
					case "int64":
					case "uint64":
					case "short":
					case "ushort":
					case "byte":
					case "sbyte":
					case "number":
						return "i_";
					case "bool":
					case "boolean":
						return "b_";
					case "datetime":
					case "date":
					case "time":
						return "dt_";
					default:
						return "fk_";		// These will be foreign key objects
				}
			}
		}
		public string sqlType {
			get {
				switch (columnType.ToLower().Trim().Replace("system.", "")) {
					case "float":
					case "decimal":
					case "single":
					case "double":
					case "quad":    // lol, for those 128-bit processors out there
						return "REAL";
					case "string":
					case "char":
						return "TEXT";
					case "int":
					case "uint":
					case "int2":
					case "int4":
					case "int8":
					case "int16":
					case "uint16":
					case "int32":
					case "uint32":
					case "long":
					case "ulong":
					case "int64":
					case "uint64":
					case "short":
					case "ushort":
					case "byte":
					case "sbyte":
					case "number":
						return "INTEGER";
					case "bool":
					case "boolean":
						return "BOOLEAN";		// this actually becomes NUMERIC, but maybe in an updated version of SQLite...
					case "datetime":
					case "date":
					case "time":
						return "DATETIME";      // this actually becomes NUMERIC, but maybe in an updated version of SQLite...
					default:
						return "INTEGER";		// The foreign key object
				}
			}
		}

		public SerializedObjectColumn(string columnName, string columnType, object columnValue) {
			this.columnName = columnName;
			this.columnType = columnType;
			this.columnValue = columnValue;
		}

		public override bool Equals(object obj) {
			if (obj == null) return false;
			SerializedObjectColumn objAsSelf = obj as SerializedObjectColumn;
			if (objAsSelf == null) return false;
			else return Equals(objAsSelf);
		}
		public bool Equals(SerializedObjectColumn other) {
			if (other == null) return false;
			return (columnName.Equals(other.columnName));
		}
		public override int GetHashCode() {
			return columnName.GetHashCode();
		}
	}

	public class SerializedObjectTableRow {
		protected int serializedID;
		protected string tablename;
		private string tablenameSQLSafe;
		protected string assemblyQualifiedName;
		protected List<SerializedObjectColumn> columns;

		public int UniqueID { get { return serializedID; } }
		public string TableName { get { return tablename; } }
		public string TableNameSQL { get { return tablenameSQLSafe; } }
		public string ObjectType { get { return assemblyQualifiedName; } }
        public List<SerializedObjectColumn> Columns { get { return columns; } }

		public SerializedObjectTableRow(int serializedID, Type objectType) {
			this.serializedID = serializedID;
			this.tablename = objectType.FullName;
			this.tablenameSQLSafe = SerializeUtilities.MakeSafeSQLType(tablename);
            this.assemblyQualifiedName = objectType.AssemblyQualifiedName;
            columns = new List<SerializedObjectColumn>();
		}

		public SerializedObjectTableRow(int serializedID, string tablename) {
			this.serializedID = serializedID;
			this.tablename = this.assemblyQualifiedName = tablename;
			this.tablenameSQLSafe = SerializeUtilities.MakeSafeSQLType(tablename);
			columns = new List<SerializedObjectColumn>();
		}

		public void AddColumn(SerializedObjectColumn colDef) {
			columns.Add(colDef);
		}
	}

	public struct SerialObjectsDefintion {
		public int UID;
		public string tablename;
		public string typename;
		public Type storageType;

		public SerialObjectsDefintion(int UID,string tablename,string typename) {
			this.UID = UID;
			this.tablename = tablename;
			this.typename = typename;
			this.storageType = Type.GetType(typename);
		}
	}
}
