using System.Collections.Generic;

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
		protected int serializedID;
		protected string tablename;
		protected List<SerializedObjectColumn> columns;

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
