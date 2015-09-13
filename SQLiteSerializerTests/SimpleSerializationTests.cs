using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLiteSerializerTests {
	[TestClass]
	public class SimpleSerializationTests : BaseTest {
		public SimpleSerializationTests() : base() { }

		[TestMethod]
		public void SimpleValueStringSerializationTest() {
			string test = "This is a test";
			MyTestSerializeRun(test);
		}

		[TestMethod]
		public void SimpleValueIntSerializationTest() {
			int number = 400;
			MyTestSerializeRun(number);
		}

		[TestMethod]
		public void SimpleClassSerializationTest() {
			SimpleTest test = new SimpleTest();
			test.Setup();

			MyTestSerializeRun(test);
		}

		[TestMethod]
		public void ComplexClassSerializationTest() {
			ComplexTest1 test = new ComplexTest1();
			test.Setup();

			MyTestSerializeRun(test);
		}

		[TestMethod]
		public void SameClassMultiplesTest() {
			MultiSameClass test = new MultiSameClass();
			test.stest1 = new SimpleTest();
			test.stest1.Setup();
			test.stest2 = test.stest1;      // these two will now equal the first, and there should only be one serialized with 3 FK links
			test.stest3 = test.stest1;

			MyTestSerializeRun(test);
		}

		[TestMethod]
		public void NullClassMultiplesTest() {
			MultiSameClass test = new MultiSameClass();
			test.stest1 = null;
			test.stest2 = null;
			test.stest3 = null;

			MyTestSerializeRun(test);
		}
	}
}
