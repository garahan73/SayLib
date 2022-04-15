using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Say32
{
    public static class SayObjectUtil {

        public static bool IsNull( this object obj ) => obj is null;

        public static T NotNull<T>(this T? obj) where T : class
        {
            if (obj == null)
                throw new NullReferenceException($"{nameof(NotNull)}() failed");

            return (T)obj;
        }

        public static string ToString( this object obj, int length ) => obj.ToString().PadRightForce(length);

        public static string? ToString(this object? obj, string format, params object?[] args)
        {
            if (obj == null) return null;

            var list = new List<object?>(1 + args.Length) { obj };
            list.AddRange(args);
            return string.Format(format, list.ToArray<object?>());
        }

        public static string? ToString(this object obj, bool condition, string format, params object?[] args)
            => condition ? ToString(obj, format, args) : null;

        public static bool IsEqualTo(this object thisObj, object thatObj)
        {
            return thisObj?.Equals(thatObj) ?? thatObj is null;
        }

        public static T ToType<T>( this object obj) => (T)(dynamic)obj;
        public static T As<T>(this object obj) => ToType<T>(obj);
        
        //public static T? AsType<T>( this object? obj ) where T: class => obj as T;


        public static string ToStringExt(this object obj, params (Type type, Func<object, string> customMethod)[] customMethods)
        {
            if (customMethods.Length == 0)
            {
                return ToStringExt(obj, null, false);
            }
            else
            {
                var customMethodMap = new Dictionary<Type, Func<object, string>>();
                customMethods.ForEach(t => customMethodMap.Add(t.type, t.customMethod));

                return ToStringExt(obj, customMethodMap, false);
            }
        }

        public static string ToStringExt(this object obj, Dictionary<Type, Func<object, string>>? customMethodMap, bool isCollectionItem)
        {
            if (customMethodMap != null)
            {
                var type = obj.GetType();
                var matchedType = customMethodMap.Keys.Where(t => t.IsAssignableFrom(type)).FirstOrDefault();

                if (matchedType != null)
                {
                    return customMethodMap[matchedType].Invoke(obj);
                }
            }

            if (obj is string s)
            {
                return isCollectionItem ? $"\"{s}\"" : s;
            }
            else if (obj is Exception ex)
            {
                return ex.ToSimpleString();
            }
            else if (obj is IEnumerable ienum)
            {
                return ToCollectionItemString(ienum, customMethodMap);
            }
            else
                return obj?.ToString() ?? "";
                
        }

        private static string ToCollectionItemString(this IEnumerable ienum, Dictionary<Type, Func<object, string>>? customMethodMap )
        {
            return ienum switch
            {
                string s => $"\"{s}\"",
                _ => $"{getTypeName()}{GetCollectionString()}"
            };

            string getTypeName() => ienum.GetType().Name.Split('<')[0];

            string GetCollectionString() => ienum switch
            {
                IDictionary dic => $"{{ {string.Join(", ", dic.Keys.Cast<object>().Select(k => $"'{k?.ToStringExt(customMethodMap, true)}' = {dic[k]?.ToStringExt(customMethodMap, true)}"))} }}",
                _ => $"[ {string.Join(", ", ienum.Cast<object>().Select(o => o?.ToStringExt(customMethodMap, true)))} ]"
            };
        }



        public static bool IsNumber( this object obj ) => obj switch
        {
            sbyte i8 => true,
            short i16 => true,
            int i32 => true,
            long i64 => true,
            byte u8 => true,
            ushort u16 => true,
            uint u32 => true,
            ulong u64 => true,
            float f => true,
            double d => true,
            _ => false,
        };

        public static bool IsFloatingNumber( this object obj ) => obj switch
        {
            float f => true,
            double d => true,
            _ => false,
        };


        public static T Lock<T>(this object obj, Func<T> func)
        {
            lock(obj)
            {
                return func();
            }
        }

        public static void Lock(this object obj, Action action )
        {
            lock (obj)
            {
                action();
            }
        }

        public static string ToSimpleString( this Exception exception ) => $"{exception.GetType().Name}: {exception.Message}";

    }
}
