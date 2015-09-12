using System;
using System.Text;
using System.Collections.Generic;

namespace SQLiteSerializerTests.TestClasses {
	[Serializable]
	public class SimpleArrayContainer {
		public SimpleArrayContainer() { }

		private static int strCount = 20;
		private string[] strs;
		public void Setup(string appender = "") {
			strs = new string[strCount];
			for (uint i = 0; i < strCount; i++) {
				strs[i] = "A string #" + i + " " + appender;
			}
		}
	}

	[Serializable]
	public class ComplexArrayContainer {
		public ComplexArrayContainer() { }

		private static int objCount = 20;
		private SimpleArrayContainer[] objs;
		public void Setup() {
			objs = new SimpleArrayContainer[objCount];
			for (uint i = 0; i < objCount; i++) {
				objs[i] = new SimpleArrayContainer();
				objs[i].Setup(" of object #" + i);
			}
		}
	}

	[Serializable]
	public class SimpleListContainer {
		public SimpleListContainer() { }

		private static int strCount = 20;
		private List<string> strs;
		public void Setup(string appender = "") {
			strs = new List<string>(strCount);
			for (uint i = 0; i < strCount; i++) {
				strs.Add("A string #" + i + " " + appender);
			}
		}
	}

	[Serializable]
	public class ComplexListContainer {
		public ComplexListContainer() { }

		private static int objCount = 20;
		private List<SimpleListContainer> objs;
		public void Setup() {
			objs = new List<SimpleListContainer>(objCount);
			for (uint i = 0; i < objCount; i++) {
				var s = new SimpleListContainer();
				s.Setup(" of object #" + i);
				objs.Add(s);
			}
		}
	}

	[Serializable]
	public class SimpledDictionaryContainer {
		public SimpledDictionaryContainer() { }

		private static int strCount = 20;
		private Dictionary<string, uint> strs;
		public void Setup(string appender = "") {
			strs = new Dictionary<string, uint>(strCount);
			for (uint i = 0; i < strCount; i++) {
				strs.Add("A string " + appender, i);
			}
		}
	}

	[Serializable]
	public class ComplexDictionaryContainer {
		public ComplexDictionaryContainer() { }

		private static int objCount = 20;
		private Dictionary<SimpleListContainer, SimpledDictionaryContainer> objs;
		public void Setup() {
			objs = new Dictionary<SimpleListContainer, SimpledDictionaryContainer>(objCount);
			for (uint i = 0; i < objCount; i++) {
				var k = new SimpleListContainer();
				k.Setup(string.Format("Dict #{0} KEY", i));
				var v = new SimpledDictionaryContainer();
				v.Setup(string.Format("Dict #{0} VALUE", i));
				objs.Add(k, v);
			}
		}
	}
}
