using System;
using System.Text;

namespace SQLiteSerializerTests {
	[Serializable]
	public class SimpleTest : IEquatable<SimpleTest> {
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

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as SimpleTest)); }
		public bool Equals(SimpleTest other) {
			return (
				text == other.text
				&& number == other.number
				&& decimalPoint == other.decimalPoint
				&& something == other.something
				&& flagOfSomething == other.flagOfSomething
            );
		}
	}

	[Serializable]
	public class ComplexTest : IEquatable<ComplexTest> {
		public string text;
		public int number;
		public float decimalPoint;

		protected SimpleTest simp1;
		protected SimpleTest simp2;

		public ComplexTest() {
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

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as ComplexTest)); }
		public bool Equals(ComplexTest other) {
			return (
				text == other.text
				&& number == other.number
				&& decimalPoint == other.decimalPoint
				&& simp1.Equals(other.simp1)
				&& simp2.Equals(other.simp2)
			);
		}
	}

	[Serializable]
	public class MultiSameClass : IEquatable<MultiSameClass> {
		public SimpleTest stest1;
		public SimpleTest stest2;
		public SimpleTest stest3;

		public MultiSameClass() { }

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as MultiSameClass)); }
		public bool Equals(MultiSameClass other) {
			return (
				stest1.Equals(other.stest1)
				&& stest2.Equals(other.stest2)
				&& stest3.Equals(other.stest3)
			);
		}
	}
}
