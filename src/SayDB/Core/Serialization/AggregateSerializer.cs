using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Say32.DB.Core.Exceptions;

namespace Say32.DB.Core.Serialization
{
    /// <summary>
    ///     The aggregate serializer
    /// </summary>
    class AggregateSerializer : BaseSerializer 
    {
        /// <summary>
        ///     List of serializers to aggregate
        /// </summary>
        private readonly List<IDbSerializer> _serializers = new List<IDbSerializer>();

        /// <summary>
        ///     The cache of actions mapped by type
        /// </summary>
        private readonly Dictionary<Type, (Action<object, BinaryWriter> serialize, Func<BinaryReader, object> deserialize)> _serializerCache
            = new Dictionary<Type, (Action<object, BinaryWriter> serialize, Func<BinaryReader, object> deserialize)>();

        /// <summary>
        ///     Quick lookup for non-serialization
        /// </summary>
        private readonly List<Type> _noSerializer = new List<Type>();

        private readonly IPlatformAdapter _platformAdapter;

        public AggregateSerializer( IPlatformAdapter platformAdapter )
        {
            _platformAdapter = platformAdapter;
        }

        /// <summary>
        ///     Clone the aggregate serializer and leave out the requesting (to avoid infinite loops)
        /// </summary>
        /// <param name="serializer">The serializer requesting the aggregate clone</param>
        /// <returns>An aggregate serializer that omits the requesting serializer</returns>
        public IDbSerializer CloneFor(IDbSerializer serializer)
        {
            var aggregateSerializer = new AggregateSerializer( _platformAdapter );
            var query = from s in _serializers where !s.GetType().Equals(serializer.GetType()) select s;
            foreach(var s in query)
            {
                aggregateSerializer.AddSerializer(s);
            }
            return aggregateSerializer;
        }

        /// <summary>
        ///     Return true if this serializer can handle the object
        /// </summary>
        /// <param name="targetType">The target</param>
        /// <returns>True if it can be serialized</returns>
        public override bool CanHandle(Type targetType)
        {
            if (_noSerializer.Contains(targetType))
                return false;

            if (_serializerCache.ContainsKey(targetType))
                return true;

            lock (((ICollection)_serializers).SyncRoot)
            {
                var canSerialize = false;
                foreach (var serializer in _serializers)
                {
                    if (serializer.CanHandle(targetType))
                    {
                        var serializer1 = serializer;
                        var serializer2 = serializer;
                        _serializerCache.Add(targetType,
                            ( serializer1.Serialize,
                                reader => serializer2.Deserialize(targetType,reader)));
                        canSerialize = true;
                        break;
                    }
                }

                if (!canSerialize)
                {
                    _noSerializer.Add(targetType);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        ///     Add a new serializer
        /// </summary>
        /// <param name="serializer">The serializer</param>
        public void AddSerializer(IDbSerializer serializer)
        {
            if (!_serializers.Contains(serializer))
            {
                _serializers.Add(serializer);
            }
        }

        /// <summary>
        ///     Serialize the object
        /// </summary>
        /// <param name="target">The target</param>
        /// <param name="writer">The writer</param>
        public override void Serialize(object target, BinaryWriter writer)
        {
            var type = target.GetType();

            if ( _platformAdapter.IsEnum( type ) )
            {
                type = Enum.GetUnderlyingType(type);
                target = Convert.ChangeType( target, type, null );
            }

            if (CanHandle(type))
            {
                _serializerCache[type].serialize(target, writer);
            }
            else
            {
                throw new SayDBSerializerException(this, target.GetType());
            }
        }

        /// <summary>
        ///     Deserialize the object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="reader">A reader to deserialize from</param>
        /// <returns>The deserialized object</returns>
        public override object Deserialize(Type type, BinaryReader reader)
        {
            var targetType = type;

            if ( _platformAdapter.IsEnum( targetType ) )
            {
                targetType = Enum.GetUnderlyingType(targetType);
            }
            if (CanHandle(targetType))
            {
                return _serializerCache[targetType].deserialize(reader);
            }
            
            throw new SayDBSerializerException(this,type);            
        }       
    }
}
