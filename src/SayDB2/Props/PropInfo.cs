using SayDB.IoJobs;
using System.Reflection;

namespace SayDB.Props;

abstract record PropInfo(DbContext DbContext)
{
    public abstract string Name { get; }
    public abstract Type PropType { get; }

    public abstract void SetValue(object obj, object? propValue);

    public abstract object? GetValue(object obj);
}

record PropertyPropInfo(DbContext DbContext, PropertyInfo Prop) : PropInfo(DbContext)
{
    public override string Name => Prop.Name;

    public override Type PropType => Prop.PropertyType;

    public override object? GetValue(object obj) => Prop.GetValue(obj);

    public override void SetValue(object obj, object? propValue) => Prop.SetValue(obj, propValue);

}

record FieldPropInfo(DbContext DbContext, FieldInfo Field) : PropInfo(DbContext)
{
    public override string Name => Field.Name;

    public override Type PropType => Field.FieldType;

    public override object? GetValue(object obj) => Field.GetValue(obj);

    public override void SetValue(object obj, object? propValue) => Field.SetValue(obj, propValue);
}