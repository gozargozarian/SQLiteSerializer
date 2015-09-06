using System.Collections.Generic;

namespace SQLiteSerializer {
	public class SerializedObjectColumn {
		public string columnName { get; set; }
		public string columnType { get; set; }
		public object columnValue { get; set; }

		public string sqlName {
			get {
				return sqlShortName + columnName;
            }
        }
		public string sqlShortName {
			get {
				switch (columnType.ToLower().Trim()) {
					case "float":
						return "f_";
					case "decimal":
					case "single":
					case "double":
					case "quad":    // lol, for those 128-bit processors out there
						return "d_";
					case "string":
						return "s_";
					case "char":
						return "c_";
					case "int":
					case "uint":
					case "int2":
					case "int4":
					case "int8":
					case "int16":
					case "int32":
					case "int64":
					case "short":
					case "ushort":
					case "byte":
					case "sbyte":
					case "number":
						return "i_";
					case "bool":
						return "b_";
					case "datetime":
					case "date":
					case "time":
						return "dt_";
					default:
						return "?_";		// I hope this blows up your code!
				}
			}
		}
		public string sqlType {
			get {
				switch (columnType.ToLower().Trim()) {
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
					case "int32":
					case "int64":
					case "short":
					case "ushort":
					case "byte":
					case "sbyte":
					case "number":
						return "INTEGER";
					case "bool":
						return "BOOLEAN";		// this actually becomes NUMERIC, but maybe in an updated version of SQLite...
					case "datetime":
					case "date":
					case "time":
						return "DATETIME";      // this actually becomes NUMERIC, but maybe in an updated version of SQLite...
					default:
						return "TEXT";        // I hope this blows up your code!
				}
			}
		}

		public SerializedObjectColumn(string columnName, string columnType, object columnValue) {
			this.columnName = columnName;
			this.columnType = columnType;
			this.columnValue = columnValue;
		}
	}

	public class SerializedObjectTable {
		protected int serializedID;
		protected string tablename;
		protected List<SerializedObjectColumn> columns;

		public int UniqueID { get { return serializedID; } }
		public string TableName { get { return tablename; } }
		public List<SerializedObjectColumn> Columns { get { return columns; } }

		public SerializedObjectTable(int serializedID, string tablename) {
			this.serializedID = serializedID;
			this.tablename = tablename;
			columns = new List<SerializedObjectColumn>();
		}

		public void AddColumn(SerializedObjectColumn colDef) {
			columns.Add(colDef);
		}
	}
}
