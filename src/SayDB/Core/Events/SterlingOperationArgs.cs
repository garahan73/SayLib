using Say32.DB.Store;
using System;

namespace Say32.DB.Core.Events
{
    /// <summary>
    ///     Notify arguments when changes happen
    /// </summary>
    public class DbOperationArgs : EventArgs 
    {
        public DbOperationArgs(DbOperation operation, DataStoreID tableID, object key)
        {
            StoreID = tableID;
            Operation = operation;
            Key = key;
        }          
  
        public DataStoreID StoreID { get; private set; }

        public object Key { get; private set; }

        public DbOperation Operation { get; private set; }
    }
}
