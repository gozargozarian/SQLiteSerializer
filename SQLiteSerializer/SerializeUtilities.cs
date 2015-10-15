using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// TODO: Investigate the portability of [MethodImpl(MethodImplOptions.AggressiveInlining)]
namespace SQLiteSerialization {
	public static class SerializeUtilities {
		#region SQLite Formatting
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeColumn(string variableName) {
			return variableName.Replace("<", "").Replace(">", "");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeSQLType(string sqlType) {
			return sqlType.Replace(".", "_").Replace("<", "").Replace(">", "").Split(new char[] { '`' })[0];
        }
		#endregion

		#region Reflection Helpers
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object CallGenericMethodWithReflection(object caller,string methodName,Type[] genericsArray,object[] parameters=null) {
			// do something awful...
			return caller.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(genericsArray)
					.Invoke(caller, parameters);
		}

		// Basic reflection does a nice job return fields, but doesn't do a great job supporting inherited fields; even using BindingFlags.FlattenHierarchy
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FieldInfo[] GetObjectFields(Type targetType) {
			// TODO: Rethink or add a toggle for BindingFlags.Static because not everyone wants all of their classes affected during one deserialize
			return targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		}
		#endregion
	}
}
