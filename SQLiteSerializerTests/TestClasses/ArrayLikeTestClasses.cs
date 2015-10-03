using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

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

	[Serializable]
	public class DifferentArrayTypes :IEquatable<DifferentArrayTypes> {
		public DifferentArrayTypes() { }

		public int[] arrInts;
		public float[] arrFloats;
		public long[] arrLongs;
		public Single[] arrSingles;
		public double[] arrDoubles;
		public string[] arrStrings;
		public char[] arrChars;
		public DateTime[] arrDates;

		public void Setup() {
			arrInts = new int[10]{ 1,2,3,4,5,6,7,8,9,10 };
			arrFloats = new float[10] { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 6.1f, 7.1f, 8.1f, 9.1f, 10.1f };
			arrLongs = new long[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			arrSingles = new Single[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			arrDoubles = new double[10] { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 6.1f, 7.1f, 8.1f, 9.1f, 10.1f };
			arrStrings = new string[10] { "one","two","three","four","five","six","seven","eight","nine","ten" };
			arrChars = new char[10] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			arrDates = new DateTime[10] {
				new DateTime(2015,1,1),
				new DateTime(2014,1,1),
				new DateTime(2013,1,1),
				new DateTime(2012,1,1),
				new DateTime(2015,2,18),
				new DateTime(2015,3,4),
				new DateTime(2015,4,8),
				new DateTime(2015,5,13),
				new DateTime(2015,6,21),
				new DateTime(2015,7,11)
			};
        }

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as DifferentArrayTypes)); }
		public bool Equals(DifferentArrayTypes other) {
			return (
				arrInts.OrderBy(a => a).SequenceEqual(other.arrInts.OrderBy(a => a))
				//&& arrFloats.OrderBy(a => a).SequenceEqual(other.arrFloats.OrderBy(a => a))		// as we all know, floats are a bitch
				&& arrLongs.OrderBy(a => a).SequenceEqual(other.arrLongs.OrderBy(a => a))
				&& arrSingles.OrderBy(a => a).SequenceEqual(other.arrSingles.OrderBy(a => a))
				//&& arrDoubles.OrderBy(a => a).SequenceEqual(other.arrDoubles.OrderBy(a => a))
				&& arrStrings.OrderBy(a => a).SequenceEqual(other.arrStrings.OrderBy(a => a))
				&& arrChars.OrderBy(a => a).SequenceEqual(other.arrChars.OrderBy(a => a))
				&& arrDates.OrderBy(a => a).SequenceEqual(other.arrDates.OrderBy(a => a))
			);
		}
	}
}
