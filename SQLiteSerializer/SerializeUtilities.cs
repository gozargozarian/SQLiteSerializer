using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

// TODO: Investigate the portability of [MethodImpl(MethodImplOptions.AggressiveInlining)]
namespace SQLiteSerialization {
	public static class SerializeUtilities {
		static Regex safeSQLTypeRegEx = new Regex("");

		#region SQLite Formatting
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeColumn(string variableName) {
			return variableName.Replace("<", "").Replace(">", "");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeSQLType(string sqlType) {
			StringBuilder typename = new StringBuilder( CleanTypeNameFromString(sqlType) );
			if (sqlType.Contains('`')) {
				string genericString = sqlType.Split('`')[1];
				int genericsCount = int.Parse((genericString.Contains("+") ? genericString.Split('+')[0] : genericString.Split('[')[0]));
				typename.AppendFormat("_{0}", genericString.Split('[')[0].Replace("+",""));

				genericString = genericString.Replace("[[", "[").Replace("]]", "]");
				string[] generics = genericString.Split(new char[] { '[',']' },StringSplitOptions.RemoveEmptyEntries);
				for (int index=1; index <= genericsCount; index++) {
					if (generics[index] == ",") continue;
					string subtypename = generics[index].Split(',')[0];
                    typename.AppendFormat("_{0}", MakeSafeSQLType(subtypename));
				}
            }

			return typename.ToString();
        }
		#endregion

		#region Type Detection
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSystemArrayLike(Type arraylikeType) {
			return (arraylikeType.IsArray && (arraylikeType.BaseType.FullName == "System.Array" || arraylikeType.IsAssignableFrom(typeof(Array))));
        }
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsListLike(Type listLikeType) {
			// NOTE: this method doesn't detect ancestor inheritance of List for a reason
			return (listLikeType.GetGenericArguments().Length == 1 && listLikeType.GetInterface(typeof(IEnumerable<>).FullName) != null && !listLikeType.IsArray);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDictionaryLike(Type dictLikeType) {
			// NOTE: this method doesn't detect ancestor inheritance of Dictionary for a reason
			return (dictLikeType.GetGenericArguments().Length == 2 && dictLikeType.GetInterface(typeof(IDictionary<,>).FullName) != null);
		}
		public static bool IsArrayLike(Type type) {
			return (IsSystemArrayLike(type) || IsListLike(type) || IsDictionaryLike(type));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSimpleValue(Type type) {
			// || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) return IsSimple(type.GetGenericArguments()[0])
			if (type.IsGenericType)
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
					return IsSimpleValue(type.GetGenericArguments()[0]);
				else
					return false;
			else
				return (type.IsPrimitive || type.IsEnum || type.IsValueType || type.Equals(typeof(string)));    //|| type.Equals(typeof(DateTime))
		}
		#endregion

		#region Reflection Helpers
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object CreateUninitializedObject(Type t,int arrayLength=0) {
			if (t.IsAbstract) return null;
			try {
				if (t.Name == "String")     // strings need specific handling of all the types in C#
					return "";
				else if (t.IsArray)
					return Array.CreateInstance(t.GetElementType(), arrayLength);
				else
					return Activator.CreateInstance(t); // give it a chance...
			} catch {
				// ... oh well
				return FormatterServices.GetUninitializedObject(t);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string CleanTypeNameFromString(string typeName) {
			return typeName.Replace(".", "_").Replace("<", "").Replace(">", "").Split('`')[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object CallGenericMethodWithReflection(object caller,string methodName,Type[] genericsArray,object[] parameters=null) {
			// do something awful...
			return caller.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
					.MakeGenericMethod(genericsArray)
					.Invoke(caller, parameters);
		}

		// Basic reflection does a nice job return fields, but doesn't do a great job supporting inherited fields; even using BindingFlags.FlattenHierarchy
		public static FieldInfo[] GetObjectFields(Type targetType) {
			// TODO: Rethink or add a toggle for BindingFlags.Static because not everyone wants all of their classes affected during one deserialize op
			List<FieldInfo> localfields = new List<FieldInfo>(
				targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)  //| BindingFlags.Static
			);

			if (targetType.BaseType != typeof(object)) {
				MergeFieldInfo(localfields,GetObjectFields(targetType.BaseType));
			}

			return localfields.ToArray();
		}

		// this function will assist in taking FieldInfo and de-duping it from upstream inheritance fields that already exist
		//	First param works like an "out" variable and will modify the List due to its ref properties
		//	Takes FieldInfo[] as a second param to avoid extra instantiating work
		public static void MergeFieldInfo(List<FieldInfo> mainList, FieldInfo[] addingList) {
			FieldInfoComparer fieldEqc = new FieldInfoComparer();
            foreach (FieldInfo f in addingList) {
				if (!mainList.Contains(f, fieldEqc)) { mainList.Add(f); }
			}
		}
		#endregion
	}

	class FieldInfoComparer : IEqualityComparer<FieldInfo> {
		public bool Equals(FieldInfo x, FieldInfo y) {
			return (x.Name == y.Name && x.FieldType == y.FieldType && x.Module == y.Module);
		}

		public int GetHashCode(FieldInfo obj) {
			return obj.GetHashCode();
		}
	}
}
