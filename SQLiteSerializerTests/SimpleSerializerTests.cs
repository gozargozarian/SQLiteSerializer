using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

using SQLiteSerialization;

namespace SQLiteSerializerTests {
	[Serializable]
	public class SimpleTest {
		public string text;
		public int number;
		public float decimalPoint;

		private string something;
		private bool flagOfSomething;

		public SimpleTest() {
		}

		public void Setup() {
			text = "This is Text.";
			number = 23;
			decimalPoint = 2.45f;
			something = "Private text.";
			flagOfSomething = true;
		}
	}

	[TestClass]
	public class SimpleSerializerTests {
		private SQLiteSerializer serializer;

		public SimpleSerializerTests() { }

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

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
		public void SimpleValueSerialization() {
			string test = "This is a test";
			int number = 400;

			serializer = new SQLiteSerializer();
			serializer.Serialize(test, "SimpleValueSerialization01.db");
			serializer.Serialize(number, "SimpleValueSerialization02.db");

			File.Delete("SimpleValueSerialization01.db");
			File.Delete("SimpleValueSerialization02.db");
		}

		[TestMethod]
		public void SimpleClassSerialization() {
			SimpleTest test = new SimpleTest();
			test.Setup();

			serializer = new SQLiteSerializer();
			serializer.Serialize(test, "SimpleClassSerialization.db");

			File.Delete("SimpleClassSerialization.db");
        }
	}
}
