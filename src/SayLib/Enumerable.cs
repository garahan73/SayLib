using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> action)
        {
            foreach (var item in enumerable)
            {
                await action(item);
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

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> asyncSelector)
        {
            var r = new List<TResult>();

            foreach (var item in source)
            {
                r.Add( await asyncSelector(item));
            }

            return r;
        }

    }
}
