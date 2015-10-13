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
	public class BasicTwoGenericTest<T,U> {
		public T something;
		public U another;
		public BasicTwoGenericTest() { }

		public void Setup(T thing,U twoThing) {
			something = thing;
			another = twoThing;
		}
	}

	[Serializable]
	public class SameGenericDifferentTypes<T,U> {
		public BasicGenericTest<T> first;
		public BasicGenericTest<U> second;

		public SameGenericDifferentTypes() { }

		public void Setup(T firstThing, U secondThing) {
			first = new BasicGenericTest<T>();
			first.Setup(firstThing);

			second = new BasicGenericTest<U>();
			second.Setup(secondThing);
		}
	}
}
