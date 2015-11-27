using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SQLiteSerializer;

namespace SQLiteSerializerTests {
	[TestClass]
	public class BaseTest {
		protected SQLiteSerializer.SQLiteSerializer serializer;

        protected TestContext testContextInstance;
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		[TestCleanup()]
		public void MyTestCleanup() {
			try { File.Delete(string.Format("{0}.db", testContextInstance.TestName)); } catch { }
		}

		public void MyTestSerializeRun(object testObj) {
			serializer = new SQLiteSerializer.SQLiteSerializer();
			serializer.Serialize(testObj, string.Format("{0}.db", testContextInstance.TestName));
			serializer = null;
		}

		public T MyTestDeserializeRun<T>() {
			serializer = new SQLiteSerializer.SQLiteSerializer();
			return serializer.Deserialize<T>(string.Format("{0}.db", testContextInstance.TestName));
		}
	}
}
