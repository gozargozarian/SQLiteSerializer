using System;
using System.Text;
using System.Collections.Generic;

namespace SQLiteSerializerTests {
	[Serializable]
	public class SimpleArrayContainer {
		public SimpleArrayContainer() { }

		private static int strCount = 20;
		public string[] strs;
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
		public SimpleArrayContainer[] objs;
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
		public List<string> strs;
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
		public List<SimpleListContainer> objs;
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
	public class SimpleDictionaryContainer {
		public SimpleDictionaryContainer() { }

		private static int strCount = 20;
		public Dictionary<string, uint> strs;
		public void Setup(string prepender = "") {
			strs = new Dictionary<string, uint>(strCount);
			for (uint i = 0; i < strCount; i++) {
				strs.Add(prepender + "A string #" + i, i);
			}
		}
	}

	[Serializable]
	public class ComplexDictionaryContainer {
		public ComplexDictionaryContainer() { }

		private static int objCount = 20;
		public Dictionary<SimpleListContainer, SimpleDictionaryContainer> objs;
		public void Setup() {
			objs = new Dictionary<SimpleListContainer, SimpleDictionaryContainer>(objCount);
			for (uint i = 0; i < objCount; i++) {
				var k = new SimpleListContainer();
				k.Setup(string.Format("Dict #{0} KEY:", i));
				var v = new SimpleDictionaryContainer();
				v.Setup(string.Format("Dict #{0} VALUE:", i));
				objs.Add(k, v);
			}
		}
	}
}
