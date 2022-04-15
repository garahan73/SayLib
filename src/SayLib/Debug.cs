using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public static class DebugHelper
    {
        public static object GetMessageHeader(object obj, string methodName) => obj.GetDegbugMessageHeader(methodName);

        public static string GetMessage(object obj, string methodName, Func<string> message) => obj.GetDegbugMessage(methodName, message);

        public static string GetDegbugMessageHeader(this object obj, string? methodName = null)

            => $"[{(obj is Type t ? t.Name : obj?.GetType().Name)}{methodName.ToString(".{0}()")}]";


        public static string GetDegbugMessage(this object obj, string? methodName = null, LazyValue<object>? message = null)

            => $"{GetDegbugMessageHeader(obj, methodName)} {message}";

        public static string GetDegbugMessage(this object obj, string? methodName, Func<object> message)

            => GetDegbugMessage(obj, methodName, (LazyValue<object>)message);

    }
}
