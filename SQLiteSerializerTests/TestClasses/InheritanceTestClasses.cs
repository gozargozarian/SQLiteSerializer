using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteSerializerTests {
	public class InheritedSimpleTest : SimpleTest {
		public string childObj;
		public InheritedSimpleTest() { }
		public override void Setup() {
			childObj = "This is a child object of SimpleTest";
			base.Setup();
        }

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) { return Equals((other as InheritedSimpleTest)); }
		public bool Equals(InheritedSimpleTest other) {
			return (
				base.Equals((SimpleTest)other)
				&& childObj == other.childObj
			);
		}
	}
}
