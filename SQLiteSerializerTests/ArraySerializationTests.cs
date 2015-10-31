﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteSerializerTests {
	[TestClass]
	public class ArraySerializationTests : BaseTest {
		public ArraySerializationTests() : base() {}

		#region System.Array Tests
		[TestMethod]
		[TestCategory("Arrays")]
		public void Array_BasicSerializationTest() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			SimpleArrayContainer result = MyTestDeserializeRun<SimpleArrayContainer>();
			Assert.AreEqual(cont,result);
        }

		[TestMethod]
		[TestCategory("Arrays")]
		public void Array_ArrayOfObjectsSerializationTest() {
			ComplexArrayContainer cont = new ComplexArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			ComplexArrayContainer result = MyTestDeserializeRun<ComplexArrayContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Arrays")]
		public void Array_StandaloneSerializationTest() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);

			string[] result = MyTestDeserializeRun<string[]>();
			Assert.IsTrue( (result.OrderBy(a => a).SequenceEqual(cont.strs.OrderBy(a => a))) );
		}

		[TestMethod]
		[TestCategory("Arrays")]
		public void Array_MultidimensionalArrayTest() {
			string[,] test = new string[2,2] { { "blah", "barg" }, { "eek", "scream" } };
			MyTestSerializeRun(test);

			string[,] result = MyTestDeserializeRun<string[,]>();
			Assert.IsTrue(test[0 ,0] == result[0, 0] 
				&& test[0, 1] == result[0, 1] 
				&& test[1, 0] == result[1, 0]
				&& test[1, 1] == result[1, 1]
			);
        }

		[TestMethod]
		[TestCategory("Arrays")]
		public void Array_DifferentArrayTypesTest() {
			DifferentArrayTypes test = new DifferentArrayTypes();
			test.Setup();

			MyTestSerializeRun(test);

			DifferentArrayTypes result = MyTestDeserializeRun<DifferentArrayTypes>();
			Assert.AreEqual(test, result);
		}
		#endregion

		#region IList Tests
		[TestMethod]
		[TestCategory("Lists")]
		public void List_SerializationTest() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			SimpleListContainer result = MyTestDeserializeRun<SimpleListContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Lists")]
		public void List_ListOfObjectsSerializationTest() {
			ComplexListContainer cont = new ComplexListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			ComplexListContainer result = MyTestDeserializeRun<ComplexListContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Lists")]
		public void List_StandaloneSerializationTest() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);

			List<string> result = MyTestDeserializeRun<List<string>>();
			Assert.IsTrue((result.OrderBy(a => a).SequenceEqual(cont.strs.OrderBy(a => a))));
		}
		#endregion

		#region IDictionary Tests
		[TestMethod]
		[TestCategory("Dictionaries")]
		public void Dictionary_SerializationTest() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			SimpleDictionaryContainer result = MyTestDeserializeRun<SimpleDictionaryContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Dictionaries")]
		public void Dictionary_DictionaryOfObjectsSerializationTest() {
			ComplexDictionaryContainer cont = new ComplexDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			ComplexDictionaryContainer result = MyTestDeserializeRun<ComplexDictionaryContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Dictionaries")]
		public void Dictionary_DictionaryOfArrayKeysSerializationTest() {
			ArrayKeyDictionaryContainer cont = new ArrayKeyDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			ArrayKeyDictionaryContainer result = MyTestDeserializeRun<ArrayKeyDictionaryContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Dictionaries")]
		public void Dictionary_DictionaryOfObjectsKeysOfObjectsSerializationTest() {
			VeryComplexDictionaryContainer cont = new VeryComplexDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);

			VeryComplexDictionaryContainer result = MyTestDeserializeRun<VeryComplexDictionaryContainer>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Dictionaries")]
		public void Dictionary_StandaloneSerializationTest() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);

			Dictionary<string,uint> result = MyTestDeserializeRun<Dictionary<string, uint>>();
			Assert.IsTrue(result.ContentEquals(cont.strs));
		}
		#endregion
	}
}
