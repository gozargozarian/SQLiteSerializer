using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SQLiteSerializerTests {
	[TestClass]
	public class SimpleSerializationTests : BaseTest {
		public SimpleSerializationTests() : base() { }

		[TestMethod]
		[TestCategory("Simple Tests")]
		public void Simple_ValueStringSerializationTest() {
			string test = "This is a test";
			MyTestSerializeRun(test);

			string result = MyTestDeserializeRun<string>();
			Assert.AreEqual(test, result);
        }

		[TestMethod]
		[TestCategory("Simple Tests")]
		public void Simple_ValueIntSerializationTest() {
			int test = 400;
			MyTestSerializeRun(test);

			int result = MyTestDeserializeRun<int>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		[TestCategory("Simple Tests")]
		public void Simple_ClassSerializationTest() {
			SimpleTest test = new SimpleTest();
			test.Setup();

			MyTestSerializeRun(test);

			SimpleTest result = MyTestDeserializeRun<SimpleTest>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		[TestCategory("Complex Tests")]
		public void Complex_ClassSerializationTest() {
			ComplexTest test = new ComplexTest();
			test.Setup();

			MyTestSerializeRun(test);

			ComplexTest result = MyTestDeserializeRun<ComplexTest>();
			Assert.AreEqual(test, result);
		}

		[TestMethod]
		[TestCategory("Complex Tests")]
		public void Complex_SameClassMultiplesTest() {
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
		[TestCategory("Simple Tests")]
		public void Simple_NullClassMultiplesTest() {
			MultiSameClass test = new MultiSameClass();
			test.stest1 = null;
			test.stest2 = null;
			test.stest3 = null;

			MyTestSerializeRun(test);

			MultiSameClass result = MyTestDeserializeRun<MultiSameClass>();
			Assert.IsTrue(result.stest1 == null && result.stest2 == null && result.stest3 == null);
		}

		[TestMethod]
		[TestCategory("Simple Tests")]
		public void Simple_SimilarVarNamesTest() {
			SimilarVarClass test = new SimilarVarClass();
			test.Setup();

			MyTestSerializeRun(test);

			SimilarVarClass result = MyTestDeserializeRun<SimilarVarClass>();
			Assert.AreEqual(test, result);
        }

		[TestMethod]
		[TestCategory("Complex Tests")]
		public void Complex_SameClassMultipleRefsTest() {
			SameRefsClass test = new SameRefsClass();
			test.Setup();

			MyTestSerializeRun(test);

			SameRefsClass result = MyTestDeserializeRun<SameRefsClass>();

			// NOTE: These compares do NOT use the .Equals() functionality because we want to test the references themselves
			Assert.IsTrue(test.one == test.two && result.one == result.two);
		}

		[TestMethod]
		[TestCategory("Simple Tests")]
		public void Simple_PropertiesTest() {
			SimplePropertiesClass test = new SimplePropertiesClass();
			test.Setup();

			MyTestSerializeRun(test);

			SimplePropertiesClass result = MyTestDeserializeRun<SimplePropertiesClass>();
			Assert.IsTrue(test.Equals(result));
		}

		/*** TODO: Dynamics may never be supported because the properties of the type are unknown at deserialization time.
			There would need to be a way of marking this and specifically handling it.
			Something I did with Inheritence serialization fixed dynamics... Wut?!
		***/
		[TestMethod]
		[TestCategory("Special (Possibly Not Supported)")]
		public void Special_TypesDynamicTest() {
			dynamic test = new { @something="another",@test=42 };
			MyTestSerializeRun(test);

			dynamic result = MyTestDeserializeRun<dynamic>();
			Assert.IsTrue(result.something == "another" && result.test == 42);
		}

		[TestMethod]
		[TestCategory("Special (Possibly Not Supported)")]
		public void Special_TypesManyDynamicsTest() {
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
