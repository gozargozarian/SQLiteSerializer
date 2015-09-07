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

	[Serializable]
	public class ComplexTest1 {
		public string text;
		public int number;
		public float decimalPoint;

		protected SimpleTest simp1;
		protected SimpleTest simp2;

		public ComplexTest1() {
		}

		public void Setup() {
			text = "This is Text.";
			number = 23;
			decimalPoint = 2.45f;

			simp1 = new SimpleTest();
			simp1.Setup();
			simp2 = new SimpleTest();
			simp2.Setup();
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
			serializer = null;

			try { File.Delete("SimpleValueSerialization01.db"); } catch { }
			try { File.Delete("SimpleValueSerialization02.db"); } catch { }
		}

		[TestMethod]
		public void SimpleClassSerialization() {
			SimpleTest test = new SimpleTest();
			test.Setup();

			serializer = new SQLiteSerializer();
			serializer.Serialize(test, "SimpleClassSerialization.db");
			serializer = null;

			try { File.Delete("SimpleClassSerialization.db"); } catch { }
        }

		[TestMethod]
		public void ComplexClassSerializationTest() {
			ComplexTest1 test = new ComplexTest1();
			test.Setup();

			serializer = new SQLiteSerializer();
			serializer.Serialize(test, "ComplexClassSerialization.db");
			serializer = null;

			try { File.Delete("ComplexClassSerialization.db"); } catch { }
		}
	}
}
