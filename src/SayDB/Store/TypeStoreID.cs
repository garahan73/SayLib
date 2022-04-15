using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Store
{
    internal class TypeStoreID : DataStoreID
    {
        public Type DataType { get; private set; }

        public TypeStoreID(Type type)
        {
            DataType = type;
        }

        public override string KeyName => DataType.FullName;

        public override int GetHashCode()
        {
            return DataType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null &&
                obj is TypeStoreID tid &&
                DataType == tid.DataType;
        }

        
        
    }
}
