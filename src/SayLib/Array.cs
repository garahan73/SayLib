using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Say32
{
    public static class SayArrayUtil
    {
        public static T Last<T>( this T[] array ) => array[array.Length - 1];

        public static T[] Slice<T>(this T[] array, int offset, int size)
        {
            var result = new T[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = array[offset + i];
            }
            return result;
        }

        public static bool ArrayEquals<T>(this T[] array, IEnumerable<T> enumerable)
        {
            var array2 = enumerable is T[] array_ ? array_ : enumerable.ToArray();

            if (array.Length != array2.Length) return false;

            for (int i = 0; i < array.Length; i++)
            {
                if (!Equals(array[i], array2[i]))
                    return false;
            }

            return true;
        }
    }


    [Serializable]
    public class ArraySingleItemException : Exception
    {
        public ArraySingleItemException() { }
        public ArraySingleItemException( string message ) : base(message) { }
        public ArraySingleItemException( string message, Exception inner ) : base(message, inner) { }
        protected ArraySingleItemException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
