using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Say32.Object
{
    public class PropertyWrapper
    {
        public PropertyInfo? PropertyInfo { get; }
        public FieldInfo? FieldInfo { get; }

        public PropertyWrapper(PropertyInfo prop)
        {
            Ex.Assert<ArgumentNullException>(prop != null, $"{nameof(PropertyInfo)} can't be null for creating {nameof(PropertyWrapper)}");
            PropertyInfo = prop;            
        }

        public PropertyWrapper(FieldInfo field)
        {
            Ex.Assert<ArgumentNullException>(field != null, $"{nameof(FieldInfo)} can't be null for creating {nameof(PropertyWrapper)}");
            FieldInfo = field;
        }

        public bool IsProperty => PropertyInfo != null;

        public string Name => (IsProperty ? PropertyInfo?.Name : FieldInfo?.Name) ?? throw new Exception("Both property and field name is null");

        public Type PropertyType => (IsProperty ? PropertyInfo?.PropertyType : FieldInfo?.FieldType) ?? throw new Exception("Both property and field type is null");

        public Type ObjectType => (IsProperty ? PropertyInfo?.DeclaringType : FieldInfo?.DeclaringType) ?? throw new Exception("Both property and field declaring type is null");

        public object? GetValue(object @object) => IsProperty ? PropertyInfo?.GetValue(@object) : FieldInfo?.GetValue(@object);
        public void SetValue(object @object, object value)
        {
            if (IsProperty)
                PropertyInfo?.SetValue(@object, value);
            else
                FieldInfo?.SetValue(@object, value);
        }

        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute 
            => IsProperty ? PropertyInfo.GetCustomAttribute<TAttribute>() : FieldInfo.GetCustomAttribute<TAttribute>();

        public static implicit operator PropertyWrapper(PropertyInfo prop)=> new PropertyWrapper(prop);
        public static implicit operator PropertyWrapper(FieldInfo field) => new PropertyWrapper(field);
    }
}
