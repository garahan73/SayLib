using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize
{
    class CustomDeserializer
    {
        public static bool TryToDeserialize( Type? type, XNode valueXml, out object? deserializedObject )
        {
            var deserializerMethod = GetCustomDeserializerMethod(type);

            if (deserializerMethod != null)
            {
                deserializedObject = Deserialize(deserializerMethod, valueXml);
                return true;
            }
            else
            {
                deserializedObject = null;
                return false;
            }
        }

        public static bool TryToDeserialize( Type? type, IEnumerable<XNode> valueXmls, out object? deserializedObject )
        {
            var deserializerMethod = GetCustomDeserializerMethod(type);

            if (deserializerMethod != null)
            {
                deserializedObject = Deserialize(deserializerMethod, valueXmls);
                return true;
            }
            else
            {
                deserializedObject = null;
                return false;
            }
        }

        internal static object Deserialize(MethodInfo deserializerMethod, object valueXml)
        {
            return deserializerMethod.Invoke(null, new object[] { valueXml });
        }

        public static MethodInfo? GetCustomDeserializerMethod( Type? type )
        {
            if (type != null)
            {
                var customDeserializerAttrib = type.GetCustomAttribute<XmlCustomDeserializerAttribute>();

                var method =  customDeserializerAttrib?.GetMethodInfo() ??
                    type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(m => m.GetCustomAttribute<XmlCustomDeserializerAttribute>() != null).SingleOrDefault();

                return method;
            }

            return null;
        }
    }

}
