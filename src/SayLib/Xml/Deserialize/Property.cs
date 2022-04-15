using Say32.Xml.Deserialize.Collection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize
{
    internal class PropertyDeserializer : ObjectCreatorHolder
    {
        public PropertyDeserializer(XmlTypeMap objectCreator) : base(objectCreator)
        {
        }

        internal void DeserializePropAttribute(object @object, XAttribute propAttrib)
        {
            var propName = propAttrib.Name.LocalName;

            //XmlIoDebug.Deserializer.Raw("");
            XmlIoDebug.Deserializer.Log("[PROP]", $"----> {propName} (XATTRIBUTE)");

            var objType = @object.GetType();

            var prop = XmlProperty.Create(objType, propName);
            if (prop == null)
                throw new XmlUnknownPropertyException($"Attribute '{propName}' doesn't exist as property of type '{objType.Name}'");

            var propValue = new PropValueXmlAttribDeserializer(prop, _xmlTypeMap).GetPropValue(propAttrib);
            if (propValue != null)
                prop.SetValue(@object, propValue);

            XmlIoDebug.Deserializer.Log("[PROP]", $"<---- {propName} (XATTRIBUTE) DONE\n");
        }

        internal void DeserializePropXElement(object @object, XElement propXelement)
        {
            var propName = propXelement.Name.LocalName;

            //XmlIoDebug.Deserializer.Raw("");
            XmlIoDebug.Deserializer.Log("[PROP]", $"----> {propName} (XELEMENT)");

            var prop = XmlProperty.Create(@object.GetType(), propName);

            if (prop == null)
                throw new XmlDeserializationException($"Can't find property '{propName}' from object type '{@object?.GetType().Name}'.", propXelement);

            var propValue = new PropValueXmlElementDeserializer(prop, _xmlTypeMap).GetPropValueFromPropXElement(propXelement);
            if (propValue != null)
                prop.SetValue(@object, propValue);

            XmlIoDebug.Deserializer.Log("[PROP]", $"<---- {propName} (XELEMENT) DONE\n");
        }
    }

    class PropValueDeserializer
    {
        private readonly XmlTypeMap _objectCreator;
        private readonly XmlProperty _prop;

        public PropValueDeserializer(XmlProperty prop, Dictionary<string, Type> typeMap)
        {
            _prop = prop;
            _objectCreator = new XmlTypeMap(typeMap);
        }

        public object? Deserialize(XElement objectXml) 
            
            => _prop.IsXmlAttribute || objectXml.Element(_prop.Name) == null
                ? new PropValueXmlAttribDeserializer(_prop, _objectCreator).GetPropValueFromObjectXml(objectXml)
                : new PropValueXmlElementDeserializer(_prop, _objectCreator).GetPropValue1FromObjectXElement(objectXml);
    }

    class PropValueXmlElementDeserializer
    {
        private readonly XmlTypeMap _objectCreator;
        private readonly XmlProperty _prop;

        public PropValueXmlElementDeserializer(XmlProperty prop, XmlTypeMap objectCreator)
        {
            _prop = prop;
            _objectCreator = objectCreator;
        }

        public object? GetPropValue1FromObjectXElement(XElement objectXml)
        {
            var propElement = objectXml.Element(_prop.Name);

            if (propElement == null) 
                return null;
            
            return GetPropValueFromPropXElement(propElement);
        }

        public object? GetPropValueFromPropXElement(XElement propXml)
        {
            XmlIoDebug.Deserializer.Log("PROP-VAL-XELE", $"{propXml.Name}");

            try
            {
                if (_prop.IsCollection)
                {
                    XmlIoDebug.Deserializer.SubLog($"prop is collection");
                    return new CollectionPropValueDeserializer(_prop, _objectCreator).DeserializeCollection(propXml);
                }
                else
                {
                    var propValueNodes = propXml.Nodes().Where(n => !(n is XComment));
                    
                    var count = propValueNodes.Count();
                    if (count == 0)
                        return null;


                    if (count == 1)
                        return new XNodeValueDeserializer(_objectCreator).Deserialize(propValueNodes.Single(), _prop.PropertyType);
                    else
                    {
                        if (CustomDeserializer.TryToDeserialize(_prop.PropertyType, propValueNodes, out var valueObject))
                        {
                            return valueObject;
                        }
                        else
                        {
                            throw new XmlDeserializationException("Multiple property value XMLs. But there's no custom deserializer.", propXml);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new XmlDeserializationException($"Failed to get property value of <{propXml.Name.LocalName}>", propXml, ex);
            }

        }
    }


    class PropValueXmlAttribDeserializer
    {
        private readonly XmlTypeMap _objectCreator;
        private readonly XmlProperty _prop;

        public PropValueXmlAttribDeserializer(XmlProperty prop, XmlTypeMap objectCreator)
        {
            _prop = prop;
            _objectCreator = objectCreator;
        }

        public object? GetPropValueFromObjectXml(XElement objectXml)
        {
            var xAttribute = objectXml.Attribute(_prop.Name) ?? objectXml.Attribute(_prop.Name.ToCamelCase());
            if (xAttribute == null) return null;

            XmlIoDebug.Deserializer.Log("PROP-VAL-ATTRIB", $"{xAttribute.Name} = {xAttribute.Value}");
            return GetPropValue(xAttribute);
        }

        public object? GetPropValue(XAttribute propAttrib)
        {
            try
            {
                if (CustomDeserializer.TryToDeserialize(_prop.PropertyType, new XText(propAttrib.Value), out object? deserializedObject))
                    return deserializedObject;
                else
                    return PrimitiveValueDeserializer.Deserialize(propAttrib.Value, _prop.PropertyType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get XML property attribute value. attrib = '{propAttrib}'", ex);
            }
        }
    }




}
