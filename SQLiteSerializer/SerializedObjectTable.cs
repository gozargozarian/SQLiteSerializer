﻿using System;
using System.Collections.Generic;

namespace SQLiteSerialization {
	public class SerializedObjectColumn : IEquatable<SerializedObjectColumn> {
		// TODO: Find a way to make MakeSafeSQLType() faster. It is hit many times. Store a "dirty" flag and the cached typename
		public string columnName { get; set; }
		public string columnType { get; set; }
		public object columnValue { get; set; }

		public string columnTypeSQLSafe { get { return SerializeUtilities.MakeSafeSQLType(columnType); } }
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
		protected List<SerializedObjectColumn> columns;

		public int UniqueID { get { return serializedID; } }
		public string TableName { get { return tablename; } }
		public string TableNameSQL { get { return SerializeUtilities.MakeSafeSQLType(tablename); } }
        public List<SerializedObjectColumn> Columns { get { return columns; } }

		public SerializedObjectTableRow(int serializedID, string tablename) {
			this.serializedID = serializedID;
			this.tablename = tablename;
			columns = new List<SerializedObjectColumn>();
		}

		public void AddColumn(SerializedObjectColumn colDef) {
			columns.Add(colDef);
		}
	}
}
