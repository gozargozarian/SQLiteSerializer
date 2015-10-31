using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteSerializerTests {
	[TestClass]
	public class GenericSerializationTests : BaseTest {
		public GenericSerializationTests() : base() {}
		
		[TestMethod]
		[TestCategory("Generics")]
		public void Generic_BasicSerializationTest() {
			BasicGenericTest<string> cont = new BasicGenericTest<string>();
			cont.Setup("My Test");
			MyTestSerializeRun(cont);

			BasicGenericTest<string> result = MyTestDeserializeRun<BasicGenericTest<string>>();
			Assert.IsTrue( cont.something == result.something );
        }

		[TestMethod]
		[TestCategory("Generics")]
		public void Generic_BasicTwoSerializationTest() {
			BasicTwoGenericTest<string, int> cont = new BasicTwoGenericTest<string, int>();
			cont.Setup("My Test",42);
			MyTestSerializeRun(cont);

			BasicTwoGenericTest<string, int> result = MyTestDeserializeRun<BasicTwoGenericTest<string, int>>();
			Assert.IsTrue( cont.something == result.something && cont.another == result.another );
		}

		[TestMethod]
		[TestCategory("Generics")]
		public void Generic_SameGenericDifferentTypes() {
			SameGenericDifferentTypes<string, int> cont = new SameGenericDifferentTypes<string, int>();
			cont.Setup("My string thing-a-majigger", 42);
			MyTestSerializeRun(cont);

			SameGenericDifferentTypes<string, int> result = MyTestDeserializeRun<SameGenericDifferentTypes<string, int>>();
			Assert.IsTrue(cont.first.something == result.first.something && cont.second.something == result.second.something);
		}
	}
}
