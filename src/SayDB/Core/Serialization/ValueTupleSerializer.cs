using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Say32.DB.Core.Serialization
{
    class ValueTupleSerializer : BaseSerializer
    {
        private static readonly ISet<Type> _valTupleTypes = new HashSet<Type>(
            new Type[] { typeof(ValueTuple<>), typeof(ValueTuple<,>),
                 typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
                 typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>),
                 typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>)
            }
        );

        IFormatter formatter = new BinaryFormatter();

        public override bool CanHandle(Type targetType)
        {
            // check if ValueTuple
            return targetType.IsGenericType
                && _valTupleTypes.Contains(targetType.GetGenericTypeDefinition());
        }

        public override object Deserialize(Type type, BinaryReader reader)
        {
            return formatter.Deserialize(reader.BaseStream);
        }

        public override void Serialize(object target, BinaryWriter writer)
        {
            formatter.Serialize(writer.BaseStream, target);
        }
    }
}
