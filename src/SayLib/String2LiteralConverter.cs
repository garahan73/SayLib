using Say32.Xml;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Say32
{
    public static class String2LiteralConverter
    {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        public static T ToLiteralValue<T>( this string valueString ) => (T)( ToLiteralValue(valueString, typeof(T)) ?? default);
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.

        public static object? ToLiteralValue( this string valueString, Type valueType )
        {
            if (valueString == null)
                return null;

            if (valueType == typeof(string) || valueType == typeof(object))
                return valueString;

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter != null)
            {
                Debug.WriteLine($"- converting string '{valueString}' to type '{valueType.Name}'");
                return converter.ConvertFromString(valueString);
            }

            throw new StringToLiteralConversionException($"string='{valueString}', type to convert = '{valueType.FullName}'");
        }

        public static bool TryToConvertToLiteralValue(this string valueString, Type targetType, out object? value)
        {
            try
            {
                value = ToLiteralValue(valueString, targetType);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }

        }

        public static bool TryToConvertToLiteralValue<T>(this string valueString, out T value)
        {
            try
            {
                value = ToLiteralValue<T>(valueString);
                return true;
            }
            catch
            {
#pragma warning disable CS8601 // 가능한 null 참조 할당입니다.
                value = default;
#pragma warning restore CS8601 // 가능한 null 참조 할당입니다.
                return false;
            }

        }
    }
}