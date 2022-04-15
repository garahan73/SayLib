using Say32.Object;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;

namespace Say32.Xml.Deserialize
{
    internal class ClassObjectDeserializer : ObjectCreatorHolder
    {
        private readonly Type? _type;

        public ClassObjectDeserializer( Type? type, XmlTypeMap objectCreator):base(objectCreator)
        {
            _type = type;
        }

        public object Deserialize(XElement valueXml)
        {
            var typeName = valueXml.Name.ToString();

            XmlIoDebug.Deserializer.Raw("\n\n");
            XmlIoDebug.Deserializer.Log("CLASS OBJECT", $"===============> {typeName}");
            XmlIoDebug.Deserializer.Raw(()=>$"{valueXml}", XmlIoDebugLevel.DETAIL);

            var type = _xmlTypeMap.GetTypeSafe(typeName);
            type = _xmlTypeMap.GetMoreDetailedType(type, _type);

            var @object = _xmlTypeMap.CreateEmptyObject(type);
            DeserializeProperties(@object, valueXml);

            XmlIoDebug.Deserializer.Log("CLASS OBJECT CREATED", $"<=============== {typeName}");
            XmlIoDebug.Deserializer.Raw(() => $"{valueXml}", XmlIoDebugLevel.DETAIL);
            XmlIoDebug.Deserializer.Raw("\n\n");

            return @object;
        }

        private void DeserializeProperties(object @object, XElement objectXml)
        {
            var propDeserializer = new PropertyDeserializer(_xmlTypeMap);

            // iterate XML sub elements
            foreach (var propXelement in objectXml.Elements())
            {
                propDeserializer.DeserializePropXElement(@object, propXelement);    
            }

            // iterate XML attributes
            foreach (var propAttrib in objectXml.Attributes())
            {
                propDeserializer.DeserializePropAttribute(@object, propAttrib);
            }
        }
    }

    
}
