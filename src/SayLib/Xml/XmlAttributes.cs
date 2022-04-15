using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Say32.Xml
{
    public class AsXmlAttribute : Attribute
    {
    }

    public class XmlCDataAttribute : Attribute
    {
    }

    public class XmlItemTagAttribute : Attribute
    {
        public XmlItemTagAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class XmlShowDeclaredPropertiesOnlyAttribute : Attribute
    {
        public XmlShowDeclaredPropertiesOnlyAttribute(bool enabled = true)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }

    public class XmlDefaultValueAttribute : Attribute
    {
        public XmlDefaultValueAttribute(object? value=null)
        {
            DefaultValue = value;
        }

        public object? DefaultValue { get; }
    }

    public class XmlReadOnlyAttribute : Attribute
    {

    }

    public enum CollectionXmlType
    {
        UNKNOWN, ItemsOnly, WithType, WithAll        
    }

    public class XmlCollectionAttribute : Attribute
    {
        public XmlCollectionAttribute(CollectionXmlType outputType)
        {
            XmlType = outputType;
        }

        public CollectionXmlType XmlType { get; }
    }


    public class XmlHelperMethodAttribute : Attribute
    {
        public XmlHelperMethodAttribute(Type? type, string? methodName)
        {
            Type = type;
            MethodName = methodName;
        }

        public Type? Type { get; }
        public string? MethodName { get; }

        public MethodInfo? GetMethodInfo() => Type?.GetMethod(MethodName);
    }

    public class XmlCustomSerializerAttribute : XmlHelperMethodAttribute
    {
        public XmlCustomSerializerAttribute() : base(null, null)
        {
        }

        public XmlCustomSerializerAttribute(Type type, string methodName):
            base(type, methodName)
        {
        }
    }

    public class XmlCustomDeserializerAttribute : XmlHelperMethodAttribute
    {
        public XmlCustomDeserializerAttribute() : base(null, null)
        {
        }

        public XmlCustomDeserializerAttribute(Type type, string methodName) :
            base(type, methodName)
        {
        }
    }

}
