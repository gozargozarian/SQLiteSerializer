using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteSerializerTests {
	[TestClass]
	public class GenericSerializationTests : BaseTest {
		public GenericSerializationTests() : base() {}
		
		[TestMethod]
		public void GenericBasicSerializationTest() {
			BasicGenericTest<string> cont = new BasicGenericTest<string>();
			cont.Setup("My Test");
			MyTestSerializeRun(cont);

			BasicGenericTest<string> result = MyTestDeserializeRun<BasicGenericTest<string>>();
			Assert.IsTrue( cont.something == result.something );
        }

		[TestMethod]
		public void GenericBasicTwoSerializationTest() {
			BasicTwoGenericTest<string, int> cont = new BasicTwoGenericTest<string, int>();
			cont.Setup("My Test",42);
			MyTestSerializeRun(cont);

			BasicTwoGenericTest<string, int> result = MyTestDeserializeRun<BasicTwoGenericTest<string, int>>();
			Assert.IsTrue( cont.something == result.something && cont.another == result.another );
		}
	}
}
