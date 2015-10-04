using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLiteSerializerTests {
	[TestClass]
	public class SimpleSerializationTests : BaseTest {
		public SimpleSerializationTests() : base() { }

		[TestMethod]
		public void SimpleValueStringSerializationTest() {
			string test = "This is a test";
			MyTestSerializeRun(test);

			string result = MyTestDeserializeRun<string>();
			Assert.AreEqual(test, result);
        }

		[TestMethod]
		public void SimpleValueIntSerializationTest() {
			int test = 400;
			MyTestSerializeRun(test);

			int result = MyTestDeserializeRun<int>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		public void SimpleClassSerializationTest() {
			SimpleTest test = new SimpleTest();
			test.Setup();

			MyTestSerializeRun(test);

			SimpleTest result = MyTestDeserializeRun<SimpleTest>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		public void ComplexClassSerializationTest() {
			ComplexTest test = new ComplexTest();
			test.Setup();

			MyTestSerializeRun(test);

			ComplexTest result = MyTestDeserializeRun<ComplexTest>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		public void SameClassMultiplesTest() {
			MultiSameClass test = new MultiSameClass();
			test.stest1 = new SimpleTest();
			test.stest1.Setup();
			test.stest2 = test.stest1;      // these two will now equal the first, and there should only be one serialized with 3 FK links
			test.stest3 = test.stest1;

			MyTestSerializeRun(test);

			MultiSameClass result = MyTestDeserializeRun<MultiSameClass>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		public void NullClassMultiplesTest() {
			MultiSameClass test = new MultiSameClass();
			test.stest1 = null;
			test.stest2 = null;
			test.stest3 = null;

			MyTestSerializeRun(test);

			MultiSameClass result = MyTestDeserializeRun<MultiSameClass>();
			Assert.IsTrue(result.stest1 == null && result.stest2 == null && result.stest3 == null);
		}

		[TestMethod]
		public void SimilarVarNamesTest() {
			SimilarVarClass test = new SimilarVarClass();
			test.Setup();

			MyTestSerializeRun(test);

			SimilarVarClass result = MyTestDeserializeRun<SimilarVarClass>();
			Assert.AreEqual(test, result);
        }
	}
}
