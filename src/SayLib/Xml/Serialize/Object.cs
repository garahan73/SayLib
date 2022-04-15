using Say32.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Say32.Xml.Serialize
{
    internal class ClassObjectSerializer
    {
        private readonly ClassSerializingContext _ctx;

        public ClassObjectSerializer(object @object, bool ignoreNullValue, bool declaredPropertiesOnly = false) //: base(@object, ignoreNullValue)
        {
            _ctx = new ClassSerializingContext(null, @object) { IgnoreNullOrDefaultValue = ignoreNullValue, DeclaredPropertiesOnly = declaredPropertiesOnly };

            if (_ctx.ObjectType.GetCustomAttribute<XmlShowDeclaredPropertiesOnlyAttribute>() != null)
                _ctx.DeclaredPropertiesOnly = true;
        }

        internal XElement? Serialize()
        {
            if (_ctx.Object == null) return null;

            var typeName = TypeNameExtractor.ExtractName(_ctx.ObjectType);
            XmlIoDebug.Serializer.Log("OBJECT", $"<{typeName}/>");

            return new XElement(typeName, GetPropertiesXelements());
        }


        private object GetPropertiesXelements()
            => _ctx.ObjectProperties.Select(prop => {
                var ctx = new ClassSerializingContext(prop, _ctx.Object) { IgnoreNullOrDefaultValue = _ctx.IgnoreNullOrDefaultValue };
                return new XmlPropertySerializer(ctx).Serialize();
            });

    }









}