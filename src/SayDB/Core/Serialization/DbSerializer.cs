using Say32.DB.Core;
using Say32.DB.Core.Serialization;
using Say32.DB.Serialization;
using System;
using System.IO;

#pragma warning disable CS0162

namespace Say32.DB.Core.Serialization
{
    /// <summary>
    ///     The aggregate serializer
    /// </summary>
    public class DbSerializers : IDbSerializer
    {
        internal DbSerializers(IPlatformAdapter platformAdapter)
        {
            _platformAdapter = platformAdapter;
            _serializers = new AggregateSerializer(_platformAdapter);

            _LoadDefaultSerializers();
        }

        private IPlatformAdapter _platformAdapter;

        private IDbSerializer Core => _serializers;

        private AggregateSerializer _serializers;

        private void _LoadDefaultSerializers()
        {
            // Load default serializes
            RegisterSerializer(new DefaultSerializer());
            RegisterSerializer(new ExtendedSerializer(_platformAdapter));

            RegisterCutomeSerializers();
        }

        private void RegisterCutomeSerializers()
        {
            // value tuple support: sehc
            RegisterSerializer(new ValueTupleSerializer());

            return;

            // serialize enumeration
            RegisterSerializer(
                canSerialize: type => typeof(Enum).IsAssignableFrom(type),
                serialize: (obj, bw) =>
                {
                    bw.Write(obj.GetType().AssemblyQualifiedName);
                    bw.Write(Convert.ToInt32(obj));
                },
                deserialize: (br) =>
                {
                    var type = Type.GetType(br.ReadString());
                    var name = Enum.GetName(type, br.ReadInt32());
                    return (Enum)Enum.Parse(type, name);
                }
            );
        }

        /// <summary>
        ///     Register a serializer with the system
        /// </summary>
        /// <typeparam name="T">The type of the serliaizer</typeparam>
        public void RegisterSerializer(IDbSerializer serializer)
        {
            _serializers.AddSerializer(serializer);
        }

        public void RegisterSerializer<T>(
            Func<Type, bool> canSerialize,
            Action<T, BinaryWriter> serialize,
            Func<BinaryReader, T> deserialize)
            => RegisterSerializer(new SimpleSerializer<T>(canSerialize, serialize, deserialize));


        public bool CanHandle(Type targetType) => Core.CanHandle(targetType);

        public void Serialize(object target, BinaryWriter writer) => Core.Serialize(target, writer);

        public object Deserialize(Type type, BinaryReader reader) => Core.Deserialize(type, reader);

        public bool CanHandle<T>() => Core.CanHandle<T>();

        public T Deserialize<T>(BinaryReader reader) => Core.Deserialize<T>(reader);
    }
}

namespace Say32.DB.Serialization
{
    internal class SimpleSerializer<T> : BaseSerializer
    {
        private Func<Type, bool> _canSerialize;
        private Action<T, BinaryWriter> _serialize;
        private Func<BinaryReader, T> _deserialize;

        public SimpleSerializer(
            Func<Type, bool> canSerialize,
            Action<T, BinaryWriter> serialize,
            Func<BinaryReader, T> deserialize)
        {
            _canSerialize = canSerialize;
            _serialize = serialize;
            _deserialize = deserialize;
        }

        public override bool CanHandle(Type targetType)
        {
            return _canSerialize(targetType);
        }

        public override void Serialize(object target, BinaryWriter writer)
        {
            _serialize((T)target, writer);
        }

        public override object Deserialize(Type type, BinaryReader reader)
        {
            return _deserialize(reader);
        }


    }
}
