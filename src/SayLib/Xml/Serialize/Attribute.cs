using System;
using System.Xml;
using System.Xml.Linq;

namespace Say32.Xml.Serialize
{
    class AttributeSerializer
    {
        public static XObject SerializePropAsAttribute(XmlProperty prop, object? value)
        {
            try
            {
                XmlIoDebug.Serializer.SubLog($"XML Attribute: {value}");

                if (CustomSerializer.TryToSerialize(value, prop, out var serializedXml))
                {
                    if (serializedXml is XText xtext)
                        return new XAttribute(prop.Name, xtext.Value);
                    else
                        return new XAttribute(prop.Name, serializedXml);
                    //else
                    //  throw new XmlException($"XML value by custom serializer is not text: {serializedXml}");

                }
                else
                    return new XAttribute(prop.Name, value);

            }
            catch (Exception ex)
            {
                throw new XmlException($"Failed serialize property({prop.Name}) to XML attribute", ex);
            }
        }
    }
}
