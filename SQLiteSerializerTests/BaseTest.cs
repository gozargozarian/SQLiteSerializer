using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SQLiteSerialization;

namespace SQLiteSerializerTests {
	[TestClass]
	public class BaseTest {
		protected SQLiteSerializer serializer;

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
            serializer = new SQLiteSerializer();
			serializer.Serialize(testObj, string.Format("{0}.db", testContextInstance.TestName));
			serializer = null;
		}
	}
}
