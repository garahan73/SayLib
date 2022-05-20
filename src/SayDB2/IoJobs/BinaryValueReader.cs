namespace SayDB.IoJobs;

internal record BinaryValueReader(IoJobContext JobContext, BinaryReader binaryReader)
{
    DbContext DbContext => JobContext.Collection.DbContext;

    public async Task<object?> ReadBinaryValueAsync(Type? type)
    {
        return type switch
        {
            Type t when t == typeof(bool) => binaryReader.ReadBoolean(),

            Type t when t == typeof(char) => binaryReader.ReadChar(),

            Type t when t == typeof(sbyte) => binaryReader.ReadSByte(),
            Type t when t == typeof(short) => binaryReader.ReadInt16(),
            Type t when t == typeof(int) => binaryReader.ReadInt32(),
            Type t when t == typeof(long) => binaryReader.ReadInt16(),

            Type t when t == typeof(byte) => binaryReader.ReadByte(),
            Type t when t == typeof(ushort) => binaryReader.ReadUInt16(),
            Type t when t == typeof(uint) => binaryReader.ReadUInt32(),
            Type t when t == typeof(ulong) => binaryReader.ReadUInt16(),

            Type t when t == typeof(float) => binaryReader.ReadSingle(),
            Type t when t == typeof(double) => binaryReader.ReadDouble(),

            Type t when t == typeof(string) => binaryReader.ReadString(),

            _ => await ReadLinkPrimaryKeyAsync()
        };

    }

    private async Task<object?> ReadLinkPrimaryKeyAsync()
    {
        if (!JobContext.LoadClassProperties)
            return null;

        var isNull = binaryReader.ReadBoolean();
        if (isNull)
            return null;

        var typeName = binaryReader.ReadString();
        var trying = Try.Run(() => Type.GetType(typeName));
        if (trying.Fail)
            throw new Exception($"Invalid type name: {typeName}");

        var type = trying.Result ?? throw new ToAvoidWarningException();

        var isDbType = binaryReader.ReadBoolean();

        if (isDbType)
        {
            if (!DbContext.Collections.ContainsKey(type))
                throw new Exception($"Illegal type name: '{type.FullName}'");

            var collection = DbContext.Collections[type];

            var primaryKey = await ReadBinaryValueAsync(collection.PrimaryKeyType);

            if (primaryKey == null)
                throw new Exception($"Primary key can't be null");

            var newContext = JobContext with { Collection = collection }; 

            return await new LoadJob(newContext).LoadAsync(primaryKey);

        }
        else
        {
            var length = binaryReader.ReadInt32();
            var bytes = binaryReader.ReadBytes(length);
            var stream = new MemoryStream(bytes);

            var obj = Activator.CreateInstance(type) ?? throw new ToAvoidWarningException();

            var properties = PropertiesFactory.Create(type, DbContext);
            await new PropertiesJob(JobContext, properties).StreamToObjectAsync(stream, obj);

            return obj;
        }
    }
}
