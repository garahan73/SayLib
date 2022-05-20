
using SayDB2.IoJobs;
using System.Reflection;

namespace SayDB2;

abstract record PropInfo(DbContext DbContext)
{
    public abstract string Name { get; }
    public abstract Type PropType { get; }

    public async Task ReadBinaryAndSetPropValueAsync(IoJobContext jobContext, object obj, BinaryReader binaryReader)
    {
        var propValue = await new BinaryValueReader(jobContext, binaryReader).ReadBinaryValueAsync(PropType);
        SetPropValueToObject(obj, propValue);
    }

    protected abstract void SetPropValueToObject(object obj, object? propValue);

    public async Task WriteBinaryValueAsync(object obj, BinaryWriter binaryWriter, IoJobContext jobContext)
    {
        var value = GetPropValueFromObject(obj);
        await new BinaryValueWriter(jobContext, binaryWriter).WriteBinaryValueAsync(value);
    }

    protected abstract object? GetPropValueFromObject(object obj);


}

record PropertyPropInfo(DbContext DbContext, PropertyInfo Prop) : PropInfo(DbContext)
{
    public override string Name => Prop.Name;

    public override Type PropType => Prop.PropertyType;

    protected override object? GetPropValueFromObject(object obj) => Prop.GetValue(obj);

    protected override void SetPropValueToObject(object obj, object? propValue) => Prop.SetValue(obj, propValue);
    
}

record FieldPropInfo(DbContext DbContext, FieldInfo Field) : PropInfo(DbContext)
{
    public override string Name => Field.Name;

    public override Type PropType => Field.FieldType;

    protected override object? GetPropValueFromObject(object obj) => Field.GetValue(obj);

    protected override void SetPropValueToObject(object obj, object? propValue) => Field.SetValue(obj, propValue);
}