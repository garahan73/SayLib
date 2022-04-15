
using Say32.DB.Store;
using System;

namespace Say32.DB.Core.Exceptions
{
    public class DbStoreNotFoundException : SayDBException
    {
        public DbStoreNotFoundException(DataStoreID tableID, string databaseName)
            : base(string.Format(ExceptionMessageFormat.SayDBTableNotFoundException, tableID, databaseName))
        {
        }

        public DbStoreNotFoundException(string typeName, string databaseName)
            : base(string.Format(ExceptionMessageFormat.SayDBTableNotFoundException, typeName, databaseName))
        {
        }
    }
}