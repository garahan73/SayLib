namespace SayDB.IoJobs;
internal record PropertiesJob(IoJobContext JobContext, PropInfo[] Properties)
{
    public PropertiesJob(IoJobContext JobContext)
        : this(JobContext, JobContext.Collection.Properties)
    {
    }

    public Task StreamToObjectAsync(Stream stream, object obj)
    {
        return Task.Run(async () =>
        {
            Dictionary<PropInfo, object> primaryKeyMap = new();

            var binaryReader = new BinaryReader(stream);

            foreach (var prop in Properties)
            {
                var propValue = await new BinaryValueReader(JobContext, binaryReader).ReadBinaryValueAsync(prop.PropType);
                prop.SetValue(obj, propValue);
            }
        });
    }

    public async Task<Stream> ObjectToStreamAsync(object obj)
    {
        return await Task.Run(async () =>
        {
            var memStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memStream);

            foreach (var prop in Properties)
            {
                var value = prop.GetValue(obj);
                await new BinaryValueWriter(JobContext, binaryWriter).WriteBinaryValueAsync(value);
            }

            return memStream;
        });
    }
}

