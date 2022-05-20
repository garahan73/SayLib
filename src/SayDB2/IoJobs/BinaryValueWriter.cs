namespace SayDB.IoJobs;

internal record BinaryValueWriter(IoJobContext JobContext, BinaryWriter binaryWriter)
    : IoJobContextOwner(JobContext)
{
    public async Task WriteBinaryValueAsync(object? value)
    {
        var type = value?.GetType();

        switch (type)
        {
            case Type when type == typeof(bool): binaryWriter.Write((bool?)value ?? default); break;

            case Type when type == typeof(char): binaryWriter.Write((char?)value ?? default); break;

            case Type when type == typeof(sbyte): binaryWriter.Write((sbyte?)value ?? default); break;
            case Type when type == typeof(short): binaryWriter.Write((short?)value ?? default); break;
            case Type when type == typeof(int): binaryWriter.Write((int?)value ?? default); break;
            case Type when type == typeof(long): binaryWriter.Write((long?)value ?? default); break;

            case Type when type == typeof(byte): binaryWriter.Write((byte?)value ?? default); break;
            case Type when type == typeof(ushort): binaryWriter.Write((ushort?)value ?? default); break;
            case Type when type == typeof(uint): binaryWriter.Write((uint?)value ?? default); break;
            case Type when type == typeof(ulong): binaryWriter.Write((ulong?)value ?? default); break;

            case Type when type == typeof(float): binaryWriter.Write((float?)value ?? default); break;
            case Type when type == typeof(double): binaryWriter.Write((double?)value ?? default); break;

            case Type when type == typeof(string): binaryWriter.Write((string?)value ?? ""); break;

            default: await WriteClassBinaryValueAsync(value); break;
        }
    }

    private async Task WriteClassBinaryValueAsync(object? value)
    {
        var isNull = value is null;
        binaryWriter.Write(isNull);

        if (value == null) return;

        var type = value.GetType();

        var typeName = type.AssemblyQualifiedName ?? throw new ToAvoidWarningException();
        binaryWriter.Write(typeName);

        var isDbType = DbContext.Collections.ContainsKey(type);
        binaryWriter.Write(isDbType);

        if (isDbType)
        {
            var collection = DbContext.Collections[type];

            var primaryKey = collection.GetPrimaryKey(value);
            await WriteBinaryValueAsync(primaryKey);

            var newContext = JobContext with { Collection = collection };

            await new SaveJob(newContext).SaveAsync(value);
            
        }
        else
        {
            var properties = PropertiesFactory.Create(type, DbContext);

            var stream = await new PropertiesJob(JobContext, properties).ObjectToStreamAsync(value);

            var memStream = stream as MemoryStream;
            if (memStream == null)
            {
                memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
            }

            var bytes = memStream.ToArray(); 

            binaryWriter.Write(bytes.Length);
            binaryWriter.Write(bytes);
        }
    }
}
