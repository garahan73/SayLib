using Say32.DB.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Say32.DB.Store
{
    class StoreProperty
    {
        public string Name { get; internal set; }
        public DataStoreID DeclaringStore { get; internal set; }
        public DataStoreID PropertyStore  { get; internal set; }

        public PropertyAccessor Accessor { get; internal set; }
    }

    class StoreProperty<T, TProp> : StoreProperty
    {

    }
}
