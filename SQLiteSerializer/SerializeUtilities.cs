using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// TODO: Investigate the portability of [MethodImpl(MethodImplOptions.AggressiveInlining)]
namespace SQLiteSerialization {
	public static class SerializeUtilities {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeColumn(string variableName) {
			return variableName.Replace("<", "").Replace(">", "");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string MakeSafeSQLType(string sqlType) {
			return sqlType.Replace(".", "_").Replace("<", "").Replace(">", "").Split(new char[] { '`' })[0];
        }
	}
}
