using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public static class ContentsEqualsUtil
    {
        //public static bool ContentsEquals<T>( this Array array, IEnumerable<T> items )
        //{
        //    if (array.Length != items.Count())
        //        return false;

        //    var i = 0;

        //    foreach (var item in items)
        //    {
        //        if (!TryContentsEquals(array.GetValue(i++), item))
        //            return false;
        //    }

        //    return true;
        //}


        //public static bool ContentsEquals<T>( this T[] array, IEnumerable<T> items )
        //{
        //    if (array.Length != items.Count())
        //        return false;

        //    var i = 0;

        //    foreach (var item in items)
        //    {
        //        if (!Equals(array[i++], item))
        //            return false;
        //    }

        //    return true;
        //}

        public static bool ContentsEquals( this IList list, IEnumerable items )
        {
            var items_ = items.Cast<object>();

            if (list.Count != items_.Count())
                return false;

            var i = 0;
            foreach (var item in items)
            {
                if (!TryContentsEquals(list[i++], item))
                    return false;
            }

            return true;
        }

        public static bool ContentsEquals<T>(this IList<T> list, IEnumerable<T> items)
        {
            var i = 0;

            if (list.Count != items.Count())
                return false;

            foreach (var item in items)
            {
                if (!TryContentsEquals(list[i++], item))
                    return false;
            }

            return true;
        }

        public static bool ContentsEquals( this IDictionary dic1, IDictionary dic2 )
        {
            //var i = 0;

            if (dic1.Count != dic2.Count)
                return false;

            foreach (var key1 in dic1.Keys)
            {
                if (!dic2.Contains(key1))
                    return false;

                if (!TryContentsEquals(dic1[key1], dic2[key1]))
                    return false;
            }

            return true;
        }

        private static bool TryContentsEquals(object? obj1, object? obj2)
        {
            if (obj1 is Array array1 && obj2 is IEnumerable e1)
                return ContentsEquals(array1, e1);
            
            if (obj2 is Array array2 && obj1 is IEnumerable e2)
                return ContentsEquals(array2, e2);

            if (obj1 is IList list1 && obj2 is IEnumerable e11)
                return ContentsEquals(list1, e11);

            if (obj2 is IList list2 && obj1 is IEnumerable e222)
                return ContentsEquals(list2, e222);

            if (obj1 is IDictionary dic1 && obj2 is IDictionary dic2)
                return ContentsEquals(dic1, dic2);

            return Equals(obj1, obj2);

        }
    }
}
