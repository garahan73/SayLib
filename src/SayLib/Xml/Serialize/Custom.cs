using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Say32.Xml.Serialize
{
    class CustomSerializer
    {
        public static bool TryToSerialize(object? value, XmlProperty? prop, out XObject? serializedXml)
        {
            try
            {

                if (value == null)
                {
                    serializedXml = null;
                    return true;
                }

                object? serializedObj = null;

                var type = prop?.PropertyType ?? value.GetType();

                var customSerializerAttrib = type.GetCustomAttribute<XmlCustomSerializerAttribute>() ?? value.GetType().GetCustomAttribute<XmlCustomSerializerAttribute>();

                var methodInfo = customSerializerAttrib?.GetMethodInfo();

                if (methodInfo != null)
                {
                    XmlIoDebug.Serializer.SubLog("Invoking custom serializer method(static)");
                    serializedObj = methodInfo.Invoke(null, new object[] { value });
                    serializedXml = toXObject(serializedObj);
                    return true;
                }


                methodInfo = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<XmlCustomSerializerAttribute>() != null).SingleOrDefault();

                if (methodInfo != null)
                {
                    XmlIoDebug.Serializer.SubLog("Invoking custom serializer method(instance)");
                    serializedObj = methodInfo.Invoke(value, null);
                    serializedXml = toXObject(serializedObj);
                    return true;
                }

                serializedXml = null;
                return false;

            }
            catch (Exception ex)
            {
                throw new XmlSerializationException($"Custom serializing failed. prop. = {prop?.Name}, value = {value}", ex);
            }


            static XObject toXObject( object serializedObj )
            {
                return serializedObj as XObject ?? new XText(serializedObj.ToString());
            }
        }
    }
}
