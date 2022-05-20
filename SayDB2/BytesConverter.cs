using SayDB.IoJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SayDB;
internal record BytesConverter(IoJobContext JobContext, PropInfo[] Properties)
{
    public BytesConverter(IoJobContext JobContext) 
        : this(JobContext, JobContext.Collection.Properties)
    {
    }

    public Task FillObjectPropValuesAsync(byte[] bytes, object obj)
    {
        return Task.Run(async () =>
        {
            Dictionary<PropInfo, object> primaryKeyMap = new();

            var binaryReader = new BinaryReader(new MemoryStream(bytes));

            foreach (var prop in Properties)
            {
                await prop.ReadBinaryAndSetPropValueAsync(JobContext, obj, binaryReader);
            }
        });
    }

    public Task<byte[]> ToBytesAsync(object obj)
    {
        return Task.Run(async () =>
        {
            var memStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memStream);

            foreach (var prop in Properties)
            {
                await prop.WriteBinaryValueAsync(obj, binaryWriter, JobContext);
            }

            return memStream.ToArray();
        });
    }
}

record ToObjectStage1Result(object Obj, Dictionary<PropInfo, object> PrimaryKeyMap);