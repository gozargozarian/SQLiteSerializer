using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

		[TestMethod]
		public void SameClassMultipleRefsTest() {
			SameRefsClass test = new SameRefsClass();
			test.Setup();

			MyTestSerializeRun(test);

			SameRefsClass result = MyTestDeserializeRun<SameRefsClass>();

			// NOTE: These compares do NOT use the .Equals() functionality because we want to test the references themselves
			Assert.IsTrue(test.one == test.two && result.one == result.two);
		}

		// TODO: Dynamics may never be supported
		[TestMethod]
		public void SpecialTypesDynamicTest() {
			dynamic test = new { @something="another",@test=42 };
			MyTestSerializeRun(test);

			dynamic result = MyTestDeserializeRun<dynamic>();
			Assert.IsTrue(result.something == "another" && result.test == 42);
		}

		[TestMethod]
		public void SpecialTypesManyDynamicsTest() {
			List<dynamic> test = new List<dynamic>();
			dynamic thing = new { @something = "another", @test = 42 };
			test.Add(thing);
			thing = new { @what = 99.0d, @isGoing = "On?" };
			test.Add(thing);
			
			MyTestSerializeRun(test);

			List<dynamic> result = MyTestDeserializeRun<List<dynamic>>();
			Assert.IsTrue(result.Count == 2);
			Assert.IsTrue(result[0].something == "another" && result[0].test == 42);
			Assert.IsTrue(result[1].what == 99.0d && result[1].isGoing == "On?");
		}


	}
}
