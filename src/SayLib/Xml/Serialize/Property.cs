using Say32.Xml.Serialize.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Say32.Xml.Serialize
{
    class XmlPropertySerializer //: PropertySerializerBase
    {
        private readonly ClassSerializingContext _ctx;

        public XmlPropertySerializer(ClassSerializingContext ctx) //: base(prop, @object, ignoreNullValue)
        {
            _ctx = ctx;
            //Debug.WriteLine($"{_ctx._type.Name}.{_ctx.Prop.Name}");
        }

        public XObject? Serialize()
        {
            var prop = _ctx.Prop ?? throw new XmlSerializationException("Xml property is null");
            var classObject = _ctx.Object;
            var ignoreNullOrDefaultValue = _ctx.IgnoreNullOrDefaultValue;

            // skip if ignore attribute is set in property
            if (prop.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                return null;

            // don't serialize property if it's readonly
            if (prop.GetCustomAttribute<XmlReadOnlyAttribute>() != null)
                return null;

            //Debug.WriteLine($"Serializing a property '{@object.GetType().Name}.{prop.Name}({prop.PropertyType.Name}) to XML");

            XmlIoDebug.Serializer.Log($"[PROP] ---> {prop.Name}");
            XmlIoDebug.Serializer.SubLog($"{_ctx.Object.GetType().Name}.{prop.Name}", XmlIoDebugLevel.DETAIL);

            var propValue = prop.GetValue(classObject);

            if (ignoreNullOrDefaultValue && propValue == null)
            {
                XmlIoDebug.Serializer.SubLog("Property value is null. -- skiping", XmlIoDebugLevel.DETAIL);
                return null;
            }
            else //if (!ignoreNullOrDefaultValue || propValue != null)
            {
                var defaultValueAttribute = prop.GetCustomAttribute<XmlDefaultValueAttribute>();
                if (Equals(defaultValueAttribute?.DefaultValue, propValue))
                {
                    XmlIoDebug.Serializer.SubLog("Value is default -- skipping.", XmlIoDebugLevel.DETAIL);
                    return null;
                }

                var asAttribute = prop.GetCustomAttribute<XmlAttributeAttribute>() != null;

                if (asAttribute)
                {
                    try
                    {
                        return AttributeSerializer.SerializePropAsAttribute(prop, propValue);
                    }
                    catch { }
                }

                // If it fails to serialize as attribute then try XElement serializing
                XmlIoDebug.Serializer.SubLog($"XML Element");
                return CreatePropertyXElement(prop);

            }
        }



        private XElement? CreatePropertyXElement(XmlProperty prop)
        {
            var value = prop.GetValue(_ctx.Object);
            if (value == null)
                return null;


            var propValueOutputType = _ctx.PropAsXml ? XmlOutputType.AsXml : XmlOutputType.Auto;
            var ignoreNullOrDefaultValue = _ctx.IgnoreNullOrDefaultValue;

            XmlIoDebug.Serializer.SubLog($"prop type = {prop.PropertyType.Name}");
            XmlIoDebug.Serializer.SubLog($"value type = '{value?.GetType().Name}',  as xml = '{propValueOutputType}'");

            //var itemTagName = _ctx.CollectionItemXmlTagName;
            var valueXml = PropValueSerializer.Serialize(prop, value, propValueOutputType, ignoreNullOrDefaultValue);

            return new XElement(prop.Name, valueXml);
            //Debug.WriteLine(ele);
        }

    }

    class PropValueSerializer
    {
        public static object? Serialize(XmlProperty prop, object? value, XmlOutputType outputType, bool ignoreNullOrDefaultValue)
        {
            if (value == null)
            {
                XmlIoDebug.Serializer.Log($"PROP VALUE", "value is null");
                return null;
            }

            if (prop.IsCollection || value is ICollection)
            {
                XmlIoDebug.Serializer.Log($"PROP VALUE", "prop value is collection");
                return CollectionXmlCreator.Serialize(prop, (IEnumerable)value, outputType, ignoreNullOrDefaultValue).Result;
            }
            else
            {
                XmlIoDebug.Serializer.Log($"PROP VALUE", "prop value is is single value");
                return XmlValueSerializer.Serialize(value, outputType, ignoreNullOrDefaultValue, prop.GetCustomAttribute<XmlCDataAttribute>() != null);
            }
        }


    }
}
