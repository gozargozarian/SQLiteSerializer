using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteSerializerTests {
	[Serializable]
	public class BasicGenericTest<T> {
		public T something;
		public BasicGenericTest() { }

		public void Setup(T thing) {
			something = thing;
		}
	}

	[Serializable]
	public class BasicTwoGenericTest<T,K> {
		public T something;
		public K another;
		public BasicTwoGenericTest() { }

		public void Setup(T thing,K twoThing) {
			something = thing;
			another = twoThing;
		}
	}
}
