using System;
using System.Collections.Generic;

namespace SQLiteSerializer {
	public struct ArrayStorageDefinition {
		public LinearObjectType type;
		public Type linearObjectTypeName;
		public Type keyTypeName;
		public Type valueTypeName;
	}

	public enum LinearObjectType : int {
		None = 0,
		SystemArray,
		IEnumerableFamily,
		IDictionaryFamily
	}

	public class SerializedArrayItem {
		internal object key;
		internal object value;
		public SerializedArrayItem(object itemKey, object itemValue) {
			key = itemKey;
			value = itemValue;
		}
	}

	public class SerializedArray {
		protected int serializedID;
		protected LinearObjectType serializeHandling;
		protected string typename;
		protected Type keyType;
		protected Type valueType;
		protected List<SerializedArrayItem> linkedItems;

		#region Properties
		public int UniqueID { get { return serializedID; } }
        public LinearObjectType ArrayType {
			get { return serializeHandling; }
		}
		public string TypeName { get { return typename; } }
		public Type KeyType { get { return keyType; } }
		public Type ValueType { get { return valueType; } }
		public List<SerializedArrayItem> Items { get { return linkedItems; } }
		#endregion

		public SerializedArray(int serializedID, Type arraylikeType) {
			this.serializedID = serializedID;
			serializeHandling = LinearObjectType.None;
			typename = arraylikeType.FullName;
			linkedItems = new List<SerializedArrayItem>();

			Type[] genArgs = arraylikeType.GetGenericArguments();
			if (SerializeUtilities.IsSystemArrayLike(arraylikeType)) {
				serializeHandling = LinearObjectType.SystemArray;       // your basic array
				keyType = typeof(uint);
				valueType = arraylikeType.GetElementType();
            } else if (SerializeUtilities.IsListLike(arraylikeType)) {
				//.IsAssignableFrom(typeof(IEnumerable<>))
				serializeHandling = LinearObjectType.IEnumerableFamily;
				keyType = typeof(uint);
				valueType = genArgs[0];
			} else if (SerializeUtilities.IsDictionaryLike(arraylikeType)) {
				//.IsAssignableFrom(typeof(IDictionary<,>))
				serializeHandling = LinearObjectType.IDictionaryFamily;
				keyType = genArgs[0];
				valueType = genArgs[1];
			} else { throw new Exception("Constructing SerializedEnumerable: Array-like object cannot be handled by this serializer: Type " + typename); }
		}

		public void AddValues(object keyValue, object valueValue) {
			linkedItems.Add(new SerializedArrayItem(keyValue, valueValue));
		}
	}
}
