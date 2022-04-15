using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Store
{
    public abstract class DataStoreID
    {
        #region FACTORY

        public static DataStoreID FromType(Type type) => new TypeStoreID(type);
        public static DataStoreID FromType<T>() => new TypeStoreID(typeof(T));
        public static DataStoreID FromName(string name) => new NameStoreID(name);

        public static implicit operator DataStoreID( string name )=> name == null? null : FromName(name);
        public static implicit operator DataStoreID( Type type ) => type == null ? null : FromType(type);

        #endregion

        public abstract string KeyName { get; }


        public override string ToString() => $"{GetType().Name}({KeyName})";

        
    }
}
