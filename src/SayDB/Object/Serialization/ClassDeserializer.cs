using Say32.DB.Core.Exceptions;
using Say32.DB.Log;
using Say32.DB.Object;
using Say32.DB.Store.IO;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{
    public delegate void SetPropertyValueMethod(object value);

    /// <summary>
    ///     This class assists with the serialization and de-serialization of objects
    /// </summary>
    /// <remarks>
    ///     This is where the heavy lifting is done, and likely where most of the tweaks make sense
    /// </remarks>
    public class ClassDeserializer : BaseSerializer
    {
        private BinaryReader _br;

        private IoHeader _header;

        public ClassDeserializer(DbObjectType dbOjbectType, IoContext context) : base(dbOjbectType, context)
        {
        }

        internal async Task<object> CreateEmptyObject(BinaryReader br, IoHeader header = null)
        {
            _logManager.Log(DbLogLevel.Verbose, $"SayDB is deserializing object...");

            _br = br;
            _header = header ?? IoHeader.Deserialize(br);

            if (_header.IsNull)
            {
                //Debug.WriteLine($"[NULL loaded]");
                return null;
            }

            DbError.Assert<SayDBException>(_header.HasTypeInfo, $"[{nameof(ClassDeserializer.CreateEmptyObject)}] no type specified in IO header");

            _type = await _ctx.TypeManager.GetType(_header.TypeCode);

            //Debug.WriteLine($"[Creating empty object] type={_type}");
            //Debug.Indent();

            _targetObject = _dbObjectType.CreateEmptyObject(br);

            //Debug.Unindent();
            //Debug.WriteLine($"[Object created] {_targetObject}");

            return _targetObject;
        }

        public async Task DeserializeProperties()
        {
            _logManager.Log(DbLogLevel.Verbose, $"SayDB is de-serializing properties of '{_targetObject}'");

            //Debug.WriteLine($"-->> Load properties start. count = {size}");
            //Debug.Indent();

            for (int i = 0; i < _header.PropertiesCount; i++)
            {
                var propDeserializer = new PropertyDeSerializer(_dbObjectType, _ctx);
                await propDeserializer.Deserialize(_br, _targetObject);
            }

            //Debug.Unindent();
            //Debug.WriteLine($"<<-- Load properties call finished. count = {size}");
        }

    }
}