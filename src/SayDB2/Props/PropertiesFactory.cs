using System.Reflection;

namespace SayDB.Props;

internal class PropertiesFactory
{
    internal static PropInfo[] Create(Type type, DbContext dbContext)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Select(field => (PropInfo)new FieldPropInfo(dbContext, field));

        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(prop => prop.CanRead && prop.CanWrite)
            .Select(prop => new PropertyPropInfo(dbContext, prop));

        return fields.Concat(props).ToArray();

    }
}