using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public static class EnumerableExtension
    {
        public static SayList<T> ToSayList<T>(this IEnumerable<T> enumerable) => new SayList<T>(enumerable);

        public static string ToArrayString<T>(this IEnumerable<T> enumerable) => $"[ {string.Join(",", enumerable)} ]";

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        public static IEnumerable<T> If<T>( this IEnumerable<T> enumerable, bool condition, Func<IEnumerable<T>, IEnumerable<T>> then, Func<IEnumerable<T>, IEnumerable<T>>? elseFunc = null )
            => condition ? then(enumerable) : elseFunc?.Invoke(enumerable) ?? enumerable;

        public static List<T> Slice<T>( this IEnumerable<T> enumerable, int offset, int size )
        {
            return enumerable.ToList().Skip(offset).Take(size).ToList();
        }

        public static IList Slice<T>( this IEnumerable enumerable, int offset, int size )
        {
            return enumerable.Cast<object>().ToList().Skip(offset).Take(size).ToList();
        }

    }
}
