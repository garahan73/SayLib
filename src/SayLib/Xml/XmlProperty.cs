using Say32.Object;
using Say32.Xml.Deserialize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Say32.Xml
{
    class XmlProperty : PropertyWrapper
    {
        public XmlProperty(PropertyInfo prop) : base(prop) { }
        public XmlProperty(FieldInfo field) : base(field) { }

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
        public static implicit operator XmlProperty(PropertyInfo prop) => prop == null ? null : new XmlProperty(prop);
        public static implicit operator XmlProperty(FieldInfo field) => field == null ? null : new XmlProperty(field);
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.

        public static XmlProperty Create(Type type, string propName) => (XmlProperty)type.GetField(propName) ?? type.GetProperty(propName);

        public bool IsXmlAttribute => GetCustomAttribute<XmlAttributeAttribute>() != null;

        public bool IsPrimitive => PropertyType.IsPrimitiveExt();

        public bool IsCollection => PropertyType.IsArray || PropertyType.IsCollection();
        public bool IsDictionary => typeof(IDictionary).IsAssignableFrom(PropertyType);

        public string CollectionItemTag => GetCustomAttribute<XmlItemTagAttribute>()?.Name ?? XmlIoTags.ITEM_TAG;

        public object? DeserializePropValue(XElement objectXml, Dictionary<string, Type>? typeMap =null)
        {
            typeMap = typeMap ?? XmlTypeMapBuilder.BuildByReprsentativeTypes(ObjectType, PropertyType);
            return new PropValueDeserializer(this, typeMap).Deserialize(objectXml);
        }

        public TAttribute GetCustomAttributeFromPropertyAndValue<TAttribute>() where TAttribute : Attribute
            => GetCustomAttribute<TAttribute>() ?? PropertyType.GetCustomAttribute<TAttribute>();


    }

}
