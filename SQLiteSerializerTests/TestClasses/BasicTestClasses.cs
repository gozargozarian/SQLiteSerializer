using System;
using System.Text;

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

	[Serializable]
	public class MultiSameClass {
		public SimpleTest stest1;
		public SimpleTest stest2;
		public SimpleTest stest3;

		public MultiSameClass() { }
	}
}
