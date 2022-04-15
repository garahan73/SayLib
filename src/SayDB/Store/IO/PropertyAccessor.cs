using Say32.DB.Store;
using System;

namespace Say32.DB.IO
{
    /// <summary>
    ///     Accessor for a class/struct property
    /// </summary>
    public class PropertyAccessor
    {
        public PropertyAccessor(string propertyName,
                                Type propertyType,
                                Action<object, object> setter,
                                Func<object, object> getter,
                                DataStoreID storeID)
        {
            PropType = propertyType;
            SetPropValue = setter;
            GetPropValue = getter;
            PropertyName = propertyName;
            StoreID = storeID;
        }

        /// <summary>
        ///     Property type
        /// </summary>
        public Type PropType { get; private set; }

        /// <summary>
        ///     The setter for the type
        /// </summary>
        public Action<object, object> SetPropValue { get; private set; }

        /// <summary>
        ///     The getter for the type
        /// </summary>
        public Func<object, object> GetPropValue { get; private set; }

        /// <summary>
        ///     The name of the property.
        /// </summary>
        public string PropertyName { get; private set; }

        public DataStoreID StoreID { get; private set; }


    }
}
