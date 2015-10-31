using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SQLiteSerializerTests {
	[TestClass]
	public class InheritanceSerializationTests : BaseTest {
		public InheritanceSerializationTests() : base() { }

		[TestMethod]
		[TestCategory("Inheritance")]
		public void Inheritance_SimpleObjectTest() {
			InheritedSimpleTest cont = new InheritedSimpleTest();
			cont.Setup();
			MyTestSerializeRun(cont);

			InheritedSimpleTest result = MyTestDeserializeRun<InheritedSimpleTest>();
			Assert.AreEqual(cont, result);
		}

		[TestMethod]
		[TestCategory("Inheritance")]
		public void Inheritance_CustomListTest() {
			MyVeryOwnList cont = new MyVeryOwnList();
			cont.Setup();
			MyTestSerializeRun(cont);

			MyVeryOwnList result = MyTestDeserializeRun<MyVeryOwnList>();
			Assert.IsTrue(result.OrderBy(a => a).SequenceEqual(cont.OrderBy(a => a)));
			Assert.IsTrue(result.thing == cont.thing);
			Assert.IsTrue(result.another == cont.another);
		}

		[TestMethod]
		[TestCategory("Inheritance")]
		public void Inheritance_CustomDictionaryTest() {
			MyVeryOwnDictionary cont = new MyVeryOwnDictionary();
			cont.Setup();
			MyTestSerializeRun(cont);

			MyVeryOwnDictionary result = MyTestDeserializeRun<MyVeryOwnDictionary>();
			Assert.IsTrue(result.ContentEquals(cont));
			Assert.IsTrue(result.thing == cont.thing);
			Assert.IsTrue(result.another == cont.another);
		}
	}
}
