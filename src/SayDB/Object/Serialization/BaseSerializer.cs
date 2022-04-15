using Say32.DB.Core;
using Say32.DB.Core.Serialization;
using Say32.DB.Log;
using Say32.DB.Object;
using Say32.DB.Object.Properties;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;

namespace Say32.DB.Object.Serialization
{


    /// <summary>
    ///     This class assists with the serialization and de-serialization of objects
    /// </summary>
    /// <remarks>
    ///     This is where the heavy lifting is done, and likely where most of the tweaks make sense
    /// </remarks>
    public class BaseSerializer
    {
        protected readonly IoContext _ctx;

        protected IPlatformAdapter PlatformAdapter => DbWorkSpace.PlatformAdapter;

        protected LogManager _logManager => DbWorkSpace.LogManager;

        protected DbObjectTypeManager TypeCache => _ctx.DbCore.DbObjectTypeManager;

        protected IDbSerializer _serializer => DbWorkSpace.Serializers;

        internal SayDB _db => _ctx.Database;

        protected readonly DataStoreID _store;

        protected object _targetObject;
        protected Type _type;

        protected readonly DbObjectType _dbObjectType;

        protected BaseSerializer(DbObjectType dbObjectType, IoContext context)
        {
            _ctx = context;
            _dbObjectType = dbObjectType;
            _store = _dbObjectType.StoreID;
        }

    }


}