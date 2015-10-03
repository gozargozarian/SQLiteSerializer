using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLiteSerializerTests {
	[TestClass]
	public class ArraySerializationTests : BaseTest {
		public ArraySerializationTests() : base() {}

		#region System.Array Tests
		[TestMethod]
		public void ArraySerializationTest() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void ArrayOfObjectsSerializationTest() {
			ComplexArrayContainer cont = new ComplexArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void ArrayStandaloneSerializationTest() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void ArrayWithinArraySerializationTest() {

		}

		[TestMethod]
		public void ArrayDifferentArrayTypesTest() {
			DifferentArrayTypes test = new DifferentArrayTypes();
			test.Setup();

			MyTestSerializeRun(test);

			DifferentArrayTypes result = MyTestDeserializeRun<DifferentArrayTypes>();
			Assert.AreEqual(test, result);
		}
		#endregion

		#region IList Tests
		[TestMethod]
		public void ListSerializationTest() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void ListOfObjectsSerializationTest() {
			ComplexListContainer cont = new ComplexListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void ListStandaloneSerializationTest() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void ListWithinListSerializationTest() {

		}
		#endregion

		#region IDictionary Tests
		[TestMethod]
		public void DictionarySerializationTest() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void DictionaryOfObjectsSerializationTest() {
			ComplexDictionaryContainer cont = new ComplexDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}
		[TestMethod]
		public void DictionaryOfObjectsKeysOfObjectsSerializationTest() {

		}

		[TestMethod]
		public void DictionaryStandaloneSerializationTest() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void DictionaryWithinDictionarySerializationTest() {

		}
		#endregion
	}
}
