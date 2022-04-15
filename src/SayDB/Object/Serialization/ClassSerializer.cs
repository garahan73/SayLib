using Say32.DB.Log;
using Say32.DB.Object;
using Say32.DB.Store.IO;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{


    /// <summary>
    ///     This class assists with the serialization and de-serialization of objects
    /// </summary>
    /// <remarks>
    ///     This is where the heavy lifting is done, and likely where most of the tweaks make sense
    /// </remarks>
    internal class ClassSerializer : BaseSerializer
    {
        private BinaryWriter _bw;

        public ClassSerializer(DbObjectType dbObjectType, IoContext context) : base(dbObjectType, context)
        {
        }

        public async Task Start(object instance, BinaryWriter bw, IoHeader header = null)
        {
            _logManager.Log(DbLogLevel.Verbose, string.Format("SayDB is serializing type {0}", instance?.GetType().FullName ?? "NULL"),
                    null);

            if (instance == null)
            {
                IoHelper.SerializeNull(_bw, header);
                return;
            }

            _bw = bw;
            _targetObject = instance;

            _type = instance.GetType();

            header = header ?? new IoHeader();

            header.StoreKeyName = _store?.KeyName;
            header.TypeCode = await _ctx.TypeManager.GetTypeIndex(_type.AssemblyQualifiedName);

            await SerializeProperties(header);

            //Debug.WriteLine($"[Saved property value] {instance}");
        }

        protected async Task SerializeProperties(IoHeader header)
        {
            //Debug.WriteLine($"-->> Save properties start");
            //Debug.Indent();

            // now iterate the serializable properties - create a copy to avoid muti-threaded conflicts
            var propAccessors = _dbObjectType.PropertyAccessors;

            header.PropertiesCount = propAccessors.Count;
            header.Serialize(_bw);

            //Debug.WriteLine($"[props. count] {propAccessors.Count}");
            //Debug.Indent();

            foreach (var propAcccessor in propAccessors.Values)
            {
                var propValue = propAcccessor.GetPropValue(_targetObject);

                var propSerializer = new PropertySerializer(_dbObjectType, _ctx, _bw);
                await propSerializer.SerializeProperty(propAcccessor, propValue);
            }

            //Debug.Unindent();
            //Debug.Unindent();
            //Debug.WriteLine($"<<-- Save properties finished] count = {propAccessors.Count}");
        }

    }
}