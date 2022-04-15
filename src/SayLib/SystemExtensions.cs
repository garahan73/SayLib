using System;
using System.Threading.Tasks;

namespace Say32
{
    public static class SystemExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static string Remove(this string txt, string txtToRemove)
        {
            return txt.Replace(txtToRemove, "");
        }

        public static bool HasResult(this Task task)
        {
            var genType = task.GetType().GetGenericArguments()[0];
            return genType.Name != "VoidTaskResult";
        }

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = s[0] + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string ToHexString(this byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 3];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[3 * i] = (char)val;
                result[3 * i + 1] = (char)(val >> 16);
                result[3 * i + 2] = ' ';
            }
            return new string(result);
        }
    }
}
