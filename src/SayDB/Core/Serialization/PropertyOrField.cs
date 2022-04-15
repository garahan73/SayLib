using System;
using System.Diagnostics;
using System.Reflection;

namespace Say32.DB.Core.Serialization
{
    /// <summary>
    ///     Abstraction of property or field
    /// </summary>
    public class PropertyOrField 
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly FieldInfo _fieldInfo;
        private readonly IPlatformAdapter _platformAdapter;

        public PropertyOrField(object infoObject, IPlatformAdapter platformAdapter)
        {
            DbError.Assert<ArgumentNullException>(infoObject != null, "infoObject");

            if (infoObject is PropertyInfo)
            {
                _propertyInfo = (PropertyInfo)infoObject;
            }
            else if (infoObject is FieldInfo)
            {
                _fieldInfo = (FieldInfo)infoObject;
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type: {0}", infoObject.GetType()), "infoObject");
            }

            _platformAdapter = platformAdapter;
        }

        public Type PropertyFieldType
        {
            get { return _propertyInfo == null ? _fieldInfo.FieldType : _propertyInfo.PropertyType; }
        }

        public string Name
        {
            get { return _propertyInfo == null ? _fieldInfo.Name : _propertyInfo.Name; }
        }

        public Type DeclaringType
        {
            get { return _propertyInfo == null ? _fieldInfo.DeclaringType : _propertyInfo.DeclaringType; }
        }

        public object GetValue(object obj)
        {
            ////Debug.WriteLine($"{nameof(GetValue)} of property {DeclaringType.Name}.{Name}");
            return _propertyInfo != null ? _platformAdapter.GetGetMethod( _propertyInfo ).Invoke(obj, new object[] { }) : _fieldInfo.GetValue(obj);
        }

        public Action<object, object> ValueSetter
        {
            get
            {
                if (_propertyInfo != null)
                {
                    return (obj, prop) => _platformAdapter.GetSetMethod( _propertyInfo ).Invoke(obj, new[] { prop });
                }

                return (obj, prop) => _fieldInfo.SetValue(obj, prop);
            }
        }

        public Func<object,object> ValueGetter
        {
            get
            {
                if (_propertyInfo != null)
                {
                    return obj => _platformAdapter.GetGetMethod( _propertyInfo ).Invoke(obj, new object[] { });
                }

                return obj => _fieldInfo.GetValue(obj);
            }
        }

        public override int GetHashCode()
        {
            return _propertyInfo == null ? _fieldInfo.GetHashCode() : _propertyInfo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyOrField && ((PropertyOrField) obj).PropertyFieldType.Equals(PropertyFieldType);
        }

        public override string ToString()
        {
            return _propertyInfo == null ? _fieldInfo.ToString() : _propertyInfo.ToString();
        }

    }
}