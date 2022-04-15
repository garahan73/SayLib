using Say32.Object;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Say32.Xml.Serialize
{
    class ClassSerializingContext : XmlIoContext
    {
        public ClassSerializingContext(XmlProperty? xmlProperty, object @object) : base(xmlProperty, @object)
        {
        }

        public Type ObjectType => Object.GetType();

        public bool IgnoreNullOrDefaultValue { get; set; }

        // secondary properties
        public IEnumerable<XmlProperty> ObjectProperties => PropertyExtractor.GetObjectProperties(ObjectType, DeclaredPropertiesOnly);

        public object? PropertyValue => Prop?.GetValue(Object);

        //public string CollectionItemXmlTagName => Prop?.GetCustomAttribute<XmlItemTagAttribute>()?.Name ?? DEFAULT_COLLECTION_ITEM_TAG;

        public bool PropAsXml => Prop?.GetCustomAttribute<AsXmlAttribute>() != null;
        public bool UseObjectTypeAsXmlName => ObjectType.GetCustomAttribute<XmlShowDeclaredPropertiesOnlyAttribute>()?.Enabled ?? false;

    }

    

}