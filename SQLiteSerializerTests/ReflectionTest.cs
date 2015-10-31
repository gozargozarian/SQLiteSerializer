using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Runtime.Serialization;

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

		[TestMethod]
		[TestCategory("Stupid Stuff")]
		public void StupidStuff_StringFormattingTest() {
			// I just needed to see if you could replace one argument multiple times
			string thing = "dog";
			string test = string.Format("This {0} is like a fat {0}, but much bigger!", thing);

			Assert.IsTrue(test == "This dog is like a fat dog, but much bigger!");
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_PropertiesTest() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			PropertyInfo[] props = tType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			Assert.IsTrue(props.Length == 3);
        }

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_FieldsTest() {
			Garbage test = new Garbage();

			Type tType = test.GetType();
			FieldInfo[] fields = tType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			Assert.IsTrue(fields.Length == 5);
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_SetPrivatesTest() {
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
		[TestCategory("Reflection")]
		public void Reflection_SetAllFieldsTest() {
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

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_RealSerializationMethods() {
			Dictionary<int, string> test = new Dictionary<int, string>();
			for (int i = 0; i < 10; i++) {
				test.Add(i, "This is item #" + i);
			}
			MemberInfo[] info = FormatterServices.GetSerializableMembers(test.GetType());
			object[] values = FormatterServices.GetObjectData(test, info);

			Assert.IsTrue(values.Length == 10);
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_RealSerializationMethodsWithInheritance() {
			MyVeryOwnDictionary test = new MyVeryOwnDictionary();
			test.Setup();

			MemberInfo[] info = FormatterServices.GetSerializableMembers(test.GetType());
			object[] values = FormatterServices.GetObjectData(test, info);

			Assert.IsTrue(values.Length == 12);
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_WhatIsASystemArrayTest() {
			string[] strArr = new string[50];
			int[] intArr = new int[25];
			StringBuilder[] sbArr = new StringBuilder[10];

			Type saType = strArr.GetType();
			Type iaType = intArr.GetType();
			Type sbaType = sbArr.GetType();

			Assert.IsTrue(saType.FullName.IndexOf("[]") > 0);
			Assert.IsTrue(saType.IsArray);
			Assert.IsTrue(saType.BaseType.FullName == "System.Array");

			Assert.IsTrue(iaType.FullName.IndexOf("[]") > 0);
			Assert.IsTrue(iaType.IsArray);
			Assert.IsTrue(iaType.BaseType.FullName == "System.Array");

			Assert.IsTrue(sbaType.FullName.IndexOf("[]") > 0);
			Assert.IsTrue(sbaType.IsArray);
			Assert.IsTrue(sbaType.BaseType.FullName == "System.Array");
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_InheritanceFields() {
			Type t = typeof(InheritedSimpleTest);
			FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

			// We will assert false because we recongize that the above process fails us: It does not return privates of inherited objects!!
			Assert.IsFalse(fields.Length > 7);
        }

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_BaseObjectDetection() {
			BaseTest test = new BaseTest();
			Assert.IsTrue(test.GetType().BaseType == typeof(object));

			Dictionary<int, string> d = new Dictionary<int, string>();
			bool found = false;
			Type baseT = d.GetType().BaseType;
			for(;;) {
				if (baseT != typeof(object)) {
					baseT = baseT.BaseType;
				} else {
					found = true;
					break;
				}
			}
			Assert.IsTrue( found );
		}

		[TestMethod]
		[TestCategory("Reflection")]
		public void Reflection_ArrayAndGenericCastingTest() {
			int[] test = new int[5] { 1, 2, 3, 4, 5 };		// just to compare
			int[] target = helperTest<int[]>();
		}
		private T helperTest<T>() {
			int[] fromList = new int[5] { 1, 2, 3, 4, 5 };  // return this through a generic
            return (T)Convert.ChangeType(fromList, typeof(T));
		}
	}
}
