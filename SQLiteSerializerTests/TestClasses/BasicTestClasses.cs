using System;
using System.Text;

namespace SQLiteSerializerTests {
	[Serializable]
	public class SimpleTest : IEquatable<SimpleTest> {
		public string text;
		public int number;
		public float decimalPoint;
		public static string statsMcGee;

		private string something;
		private bool flagOfSomething;

		public SimpleTest() {
		}

		public virtual void Setup() {
			text = "This is Text.";
			number = 23;
			decimalPoint = 2.45f;
			something = "Private text.";
			flagOfSomething = true;
			statsMcGee = "Fo' Shizzle";
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
				&& SimpleTest.statsMcGee == "Fo' Shizzle"
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

	[Serializable]
	public class SimilarVarClass : IEquatable<SimilarVarClass> {
		public string AsomeName;
		public string BsomeName;
		
		public SimilarVarClass() { }
		public void Setup() {
			AsomeName = "A some name";
			BsomeName = "B some name";
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as SimilarVarClass)); }
		public bool Equals(SimilarVarClass other) {
			return (
				AsomeName == other.AsomeName
				&& BsomeName == other.BsomeName
			);
		}
	}

	[Serializable]
	public class SameRefsClass {
		public SimpleTest one;
		public SimpleTest two;

		public SameRefsClass() { }
		
		public void Setup() {
			one = two = new SimpleTest();
			one.Setup();
		}
	}

	[Serializable]
	public class SimplePropertiesClass {
		private string forGetter;
		public string stuff { get; set; }
		private int another { get; set; }
		public string pureGetter { get { return forGetter; } }

		public SimplePropertiesClass() { }
		public void Setup() {
			forGetter = "The Getter";
			stuff = "stuff";
			another = 42;
		}

		public bool Equals(SimplePropertiesClass other) {
			return (
				forGetter == other.forGetter
				&& stuff == other.stuff
				&& another == other.another
				&& pureGetter == other.pureGetter
            );
		}
	}
}
