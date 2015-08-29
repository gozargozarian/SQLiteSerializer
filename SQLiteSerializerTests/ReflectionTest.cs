using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace SQLiteSerializerTests {
	/// <summary>
	/// Summary description for ReflectionTest
	/// </summary>
	[TestClass]
	public class ReflectionTest {
		protected class Garbage {
			public string pubField;
			public string pubProp { get; set; }

			protected string proField;
			protected string proProp { get; set; }

			protected string PropNoSetter { get; }

			public Garbage() {
				pubField = "pubField";
				pubProp = "pubProp";
				proField = "proField";
				proProp = "proProp";
				PropNoSetter = "PropNoSetter";
			}

			public string ConfirmTestPropNoSetter() { return PropNoSetter; }
			public string ConfirmTestProProp() { return proProp; }
		}

		public ReflectionTest() {}

		#region Context Junk
		private TestContext testContextInstance;
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}
		#endregion

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void TestReflectionProperties() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			PropertyInfo[] props = tType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			Assert.IsTrue(props.Length == 3);
        }

		[TestMethod]
		public void TestReflectionFields() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			FieldInfo[] fields = tType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			Assert.IsTrue(fields.Length == 5);
		}

		[TestMethod]
		public void TestReflectionSetPrivates() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			PropertyInfo[] props = tType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
			PropertyInfo prop = props[0];
			if (prop.CanWrite)
				prop.SetValue(test, "Something New");
			prop = props[1];
			if (prop.CanWrite)
				prop.SetValue(test, "Something New");		// this throws an exception because it can't write

			Assert.IsTrue(test.ConfirmTestProProp() == "Something New");
			Assert.IsTrue(test.ConfirmTestPropNoSetter() != "Something New");
		}

		[TestMethod]
		public void TestReflectionSetAllFields() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			FieldInfo[] fields = tType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (var field in fields) {
				field.SetValue(test, "Something New");
			}

			Assert.IsTrue(test.pubField == "Something New");
			Assert.IsTrue(test.pubProp == "Something New");
			Assert.IsTrue(test.ConfirmTestPropNoSetter() == "Something New");
			Assert.IsTrue(test.ConfirmTestProProp() == "Something New");
		}
	}
}
