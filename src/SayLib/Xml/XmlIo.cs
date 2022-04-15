using Say32.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Say32.Xml
{
    enum XmlOutputType { AsXml, AsPrimitive, Auto };

    class XmlIoTags
    {
        public const string ITEM_TAG = "Item";
        public const string ITEMS_TAG = "Items";
        public const string KEY_TAG = "Key";
        public const string VALUE_TAG = "Value";
    }

    abstract class XmlIoContext
    {
        public XmlIoContext(XmlProperty? prop, object @object)
        {
            if (@object == null) throw new XmlIoContextException($"Target object can't be null");

            Prop = prop;
            Object = @object;
        }
                
        public bool DeclaredPropertiesOnly { get; set; }

        public XmlProperty? Prop { get; }

        public object Object { get;  }
    }

    class PropertyExtractor
    {
        private const BindingFlags PROP_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance;

        public static BindingFlags GetPropertiesBindingFlags(bool declaredPropertiesOnly) => PROP_BINDING_FLAGS | (declaredPropertiesOnly ? BindingFlags.DeclaredOnly : BindingFlags.Default);

        public static IEnumerable<XmlProperty> GetObjectProperties( Type objectType, bool declaredPropertiesOnly )
        {
            var bindingFlog = GetPropertiesBindingFlags(declaredPropertiesOnly);

            return objectType.GetProperties(bindingFlog).Where(p => p.CanRead && p.GetSetMethod() != null).Select(p => new XmlProperty(p)).
                           Concat(objectType.GetFields(bindingFlog).Select(p => new XmlProperty(p)));
        }
    }

    class TypeNameExtractor
    {
        public static string ExtractName(Type type) => type.Name.Split('`')[0].Split('+').Last();
    }


    [Serializable]
    public class XmlIoContextException : Exception
    {
        public XmlIoContextException() { }
        public XmlIoContextException(string message) : base(message) { }
        public XmlIoContextException(string message, Exception inner) : base(message, inner) { }
        protected XmlIoContextException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}