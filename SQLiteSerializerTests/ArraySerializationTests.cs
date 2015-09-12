using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLiteSerializerTests {
	[TestClass]
	public class ArraySerializationTests : BaseTest {
		public ArraySerializationTests() : base() {}

		#region System.Array Tests
		[TestMethod]
		public void TestArraySerialization() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void TestArrayOfObjectsSerialization() {
			ComplexArrayContainer cont = new ComplexArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void TestStandaloneArraySerialization() {
			SimpleArrayContainer cont = new SimpleArrayContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void TestArrayWithinArraySerialization() {

		}
		#endregion

		#region IList Tests
		[TestMethod]
		public void TestListSerialization() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void TestListOfObjectsSerialization() {
			ComplexListContainer cont = new ComplexListContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void TestStandaloneListSerialization() {
			SimpleListContainer cont = new SimpleListContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void TestListWithinListSerialization() {

		}
		#endregion

		#region IDictionary Tests
		[TestMethod]
		public void TestDictionarySerialization() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}

		[TestMethod]
		public void TestDictionaryOfObjectsSerialization() {
			ComplexDictionaryContainer cont = new ComplexDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont);
		}
		[TestMethod]
		public void TestDictionaryOfObjectsKeysOfObjectsSerialization() {

		}

		[TestMethod]
		public void TestStandaloneDictionarySerialization() {
			SimpleDictionaryContainer cont = new SimpleDictionaryContainer();
			cont.Setup();
			MyTestSerializeRun(cont.strs);
		}

		[TestMethod]
		public void TestDictionaryWithinDictionarySerialization() {

		}
		#endregion
	}
}
