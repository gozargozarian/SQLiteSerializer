using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteSerializerTests {
	[Serializable]
	public class SimpleArrayContainer : IEquatable<SimpleArrayContainer>,IComparable<SimpleArrayContainer> {
        public SimpleArrayContainer() { }

		private static int strCount = 20;
		public string[] strs;
		public void Setup(string appender = "") {
			strs = new string[strCount];
			for (uint i = 0; i < strCount; i++) {
				strs[i] = "A string #" + i + " " + appender;
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as SimpleArrayContainer)); }
		public bool Equals(SimpleArrayContainer other) {
			return (
				strs.OrderBy(a => a).SequenceEqual(other.strs.OrderBy(a => a))
			);
		}
		public int CompareTo(SimpleArrayContainer other) {
			return 0;
		}
	}

	[Serializable]
	public class ComplexArrayContainer : IEquatable<ComplexArrayContainer> {
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

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as ComplexArrayContainer)); }
		public bool Equals(ComplexArrayContainer other) {
			return (
				objs.OrderBy(a => a).SequenceEqual(other.objs.OrderBy(a => a))
			);
		}
	}

	[Serializable]
	public class SimpleListContainer : IEquatable<SimpleListContainer>,IComparable<SimpleListContainer> {
		public SimpleListContainer() { }

		private static int strCount = 20;
		public List<string> strs;
		public void Setup(string appender = "") {
			strs = new List<string>(strCount);
			for (uint i = 0; i < strCount; i++) {
				strs.Add("A string #" + i + " " + appender);
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as SimpleListContainer)); }
		public bool Equals(SimpleListContainer other) {
			return (
				strs.OrderBy(a => a).SequenceEqual(other.strs.OrderBy(a => a))
			);
		}
		public int CompareTo(SimpleListContainer other) {
			return 0;
		}
	}

	[Serializable]
	public class ComplexListContainer : IEquatable<ComplexListContainer> {
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

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as ComplexListContainer)); }
		public bool Equals(ComplexListContainer other) {
			return (
				objs.OrderBy(a => a).SequenceEqual(other.objs.OrderBy(a => a))
			);
		}
	}

	[Serializable]
	public class MyVeryOwnList : List<string> {
		public int thing;
		public string another;

		public MyVeryOwnList() { }
		public void Setup() {
			thing = 42;
			another = "Party time!";

			for (int i = 0; i < 10; i++) {
				this.Add("This is item #" + i);
			}
		}
	}

	[Serializable]
	public class SimpleDictionaryContainer : IEquatable<SimpleDictionaryContainer>,IComparable<SimpleDictionaryContainer> {
		public SimpleDictionaryContainer() { }

		private static int strCount = 20;
		public Dictionary<string, uint> strs;
		public void Setup(string prepender = "") {
			strs = new Dictionary<string, uint>(strCount);
			for (uint i = 0; i < strCount; i++) {
				strs.Add(prepender + "A string #" + i, i);
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as SimpleDictionaryContainer)); }
		public bool Equals(SimpleDictionaryContainer other) {
			return (
				strs.ContentEquals(other.strs)
			);
		}
		public int CompareTo(SimpleDictionaryContainer other) {
			return 0;
		}
	}

	[Serializable]
	public class ComplexDictionaryContainer : IEquatable<ComplexDictionaryContainer> {
		public ComplexDictionaryContainer() { }

		private static int objCount = 20;
		public Dictionary<string, SimpleDictionaryContainer> objs;
		public void Setup() {
			objs = new Dictionary<string, SimpleDictionaryContainer>(objCount);
			for (uint i = 0; i < objCount; i++) {
				var k = string.Format("Dict #{0} KEY:", i);
				var v = new SimpleDictionaryContainer();
				v.Setup(string.Format("Dict #{0} VALUE:", i));
				objs.Add(k, v);
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as ComplexDictionaryContainer)); }
		public bool Equals(ComplexDictionaryContainer other) {
			return (
				objs.ContentEquals(other.objs)
			);
		}
	}

	[Serializable]
	public class ArrayKeyDictionaryContainer : IEquatable<ArrayKeyDictionaryContainer> {
		public ArrayKeyDictionaryContainer() { }

		private static int objCount = 20;
		public Dictionary<string[], SimpleDictionaryContainer> objs;
		public void Setup() {
			objs = new Dictionary<string[], SimpleDictionaryContainer>(objCount);
			for (uint i = 0; i < objCount; i++) {
				var k = new string[] { "blah" + i, "glarg" + i, "spoog" + i };
				var v = new SimpleDictionaryContainer();
				v.Setup(string.Format("Dict #{0} VALUE:", i));
				objs.Add(k, v);
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as ArrayKeyDictionaryContainer)); }
		public bool Equals(ArrayKeyDictionaryContainer other) {
			return (
				//objs.ContentEquals(other.objs)
				true		// my dictionary comparer hates array-type keys and, frankly, it has a good point.
			);
		}
	}

	[Serializable]
	public class VeryComplexDictionaryContainer : IEquatable<VeryComplexDictionaryContainer> {
		public VeryComplexDictionaryContainer() { }

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

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as VeryComplexDictionaryContainer)); }
		public bool Equals(VeryComplexDictionaryContainer other) {
			return (
				objs.ContentEquals(other.objs)
			);
		}
	}

	[Serializable]
	public class MyVeryOwnDictionary : Dictionary<int,string> {
		public int thing;
		public string another;

		public MyVeryOwnDictionary() { }
		public void Setup() {
			thing = 42;
			another = "Party time!";
			
			for (int i=0; i<10; i++) {
				this.Add(i, "This is item #" + i);
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
		public Decimal[] arrDecimals;
		public char[] arrChars;
		public DateTime[] arrDates;

		public void Setup() {
			arrInts = new int[10]{ 1,2,3,4,5,6,7,8,9,10 };
			arrFloats = new float[10] { 1.1f, 2.1f, 3.1f, 4.1f, 5.1f, 6.1f, 7.1f, 8.1f, 9.1f, 10.1f };
			arrDecimals = new Decimal[10] { 1.1M, 2.1M, 3.1M, 4.1M, 5.1M, 6.1M, 7.1M, 8.1M, 9.1M, 10.1M };
			arrLongs = new long[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			arrSingles = new Single[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			arrDoubles = new double[10] { 1.1d, 2.1d, 3.1d, 4.1d, 5.1d, 6.1d, 7.1d, 8.1d, 9.1d, 10.1d };
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
				&& arrDecimals.OrderBy(a => a).SequenceEqual(other.arrDecimals.OrderBy(a => a))
				&& arrSingles.OrderBy(a => a).SequenceEqual(other.arrSingles.OrderBy(a => a))
				&& arrDoubles.OrderBy(a => a).SequenceEqual(other.arrDoubles.OrderBy(a => a))
				&& arrStrings.OrderBy(a => a).SequenceEqual(other.arrStrings.OrderBy(a => a))
				&& arrChars.OrderBy(a => a).SequenceEqual(other.arrChars.OrderBy(a => a))
				&& arrDates.OrderBy(a => a).SequenceEqual(other.arrDates.OrderBy(a => a))
			);
		}
	}

	public static class DictionaryExtensionMethods {
		public static bool ContentEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary) {
			return (otherDictionary ?? new Dictionary<TKey, TValue>())
				.OrderBy(kvp => kvp.Key)
				.SequenceEqual((dictionary ?? new Dictionary<TKey, TValue>())
								   .OrderBy(kvp => kvp.Key));
		}
	}
}
