using Say32.Xml;
using Say32.Xml.Deserialize;
using Say32.Xml.Serialize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Say32
{
    public static class XmlUtil
    {
        //public static T ImportFromXml<T>(this T @object, XElement xml, params Type[] representativeTypes) => (T)new ObjectDeserializer(xml, typeof(T), representativeTypes).Deserialize(@object);
        //public static T ImportFromXml<T>(this T @object, string xmlText, params Type[] representativeTypes) => ImportFromXml<T>(@object, XElement.Parse(xmlText), representativeTypes);

        public static object ToXml<T>(this T @object, bool ignoreNullValue=true) => XmlValueSerializer.Serialize(@object, XmlOutputType.Auto, ignoreNullValue, false) ?? throw new Exception();

        public static XElement? ToXElement<T>(this T @object, bool ignoreNullValue = true) => (XElement?)ToXml<T>(@object, ignoreNullValue);

        //new XElementObjectSerializer(@object, ignoreNullValue).Serialize();
        public static string ToXmlString<T>(this T @object, bool ignoreNullValue = true) => ToXml(@object, ignoreNullValue).ToString();

        public static XObject? SerializeSingleClass<T>( this T @object, bool ignoreNullValue = true, bool declaredPropertiesOnly = false )
            where T: class
            => new ClassObjectSerializer(@object, ignoreNullValue, declaredPropertiesOnly).Serialize();

        public static T DeserializeSingleClass<T>( this XElement ele, XmlTypeMap typeMap ) => (T)new ClassObjectDeserializer(typeof(T), typeMap).Deserialize(ele);

        public static string? GetValueFromXml( this XElement xml, string itemName, bool includingCamelCaseAttributeName = true )
        {
            return xml.Element(itemName)?.Value ?? GetXAttributeValue(xml, itemName, includingCamelCaseAttributeName);
        }

        public static string? GetXAttributeValue( this XElement xelement, string itemName, bool includingCamelCaseAttributeName = true )
        {
            return xelement.Attribute(itemName)?.Value ?? (includingCamelCaseAttributeName ? xelement.Attribute(itemName.ToCamelCase())?.Value : null);
        }

        public static T ToObject<T>( this XObject xml, XmlTypeMap? typeMap = null )
        {
            try
            {
                if (xml is XNode xnode)
                {
                    var targetType = typeof(T);
                    typeMap = typeMap ?? new XmlTypeMap();

                    if (xnode is XElement xele)
                    {
                        var typeName = xele.Name.LocalName;
                        targetType = typeMap.GetMoreDetailedType(targetType, typeName);
                    }

                    if (CustomDeserializer.TryToDeserialize(targetType, xnode, out var deserializedObject))
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                        return (T)deserializedObject;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
                    else
                    {   
                        if (!typeMap.ContainsKey(targetType.Name))
                            typeMap.Add(targetType.Name, targetType);

#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                        return (T)new XNodeValueDeserializer(typeMap).Deserialize(xnode, typeof(T));
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
                    }
                }
                else
                {
                    return (T)PrimitiveValueDeserializer.Deserialize(xml.ToString(), typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw new XmlParsingException($"Target Object Type = '{typeof(T)}'", ex);
            }

        }
                
        public static T XmlToObject<T>( this string xmlText, XmlTypeMap? typeMap = null )
        {
            if (xmlText.TrimStart().StartsWith("<") )
            {
                var xele = XElement.Parse(xmlText);
                return ToObject<T>(xele, typeMap);
            }
            else
            {
                return ToObject<T>(new XText(xmlText), typeMap);
            }
        }

        public static IEnumerable<T> ToObjects<T>(this XElement xml, string xmlTagName, XmlTypeMap? typeMap = null ) 
            => xml.Elements(xmlTagName).Select(xele => ToObject<T>(xele, typeMap));

        public static IEnumerable<T> ToObjects<T>(string xmlText, string xmlTagName, XmlTypeMap? typeMap = null ) 
            => ToObjects<T>(XElement.Parse(xmlText), xmlTagName, typeMap);

        public static XAttribute? ToXAttribute(this object value, string name, bool ignoreNullValue = true) => ignoreNullValue && value == null ? null : new XAttribute(name, value);        

    }


    [Serializable]
    public class XmlParsingException : Exception
    {
        public XmlParsingException() { }
        public XmlParsingException( string message ) : base(message) { }
        public XmlParsingException( string message, Exception inner ) : base(message, inner) { }
        protected XmlParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }

}
