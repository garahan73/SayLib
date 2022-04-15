using Say32.DB.Core.Exceptions;
using Say32.DB.Core.Serialization;
using Say32.DB.Driver;
using Say32.DB.Log;
using Say32.DB.Server;
using Say32.DB.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Core
{
    /// <summary>
    ///     The SayDB database manager
    /// </summary>
    internal class DbWorkSpaceCore : IDisposable
    {
        private bool _activated;
        private readonly object _lock = new object();

        /// <summary>
        ///     Master list of databases
        /// </summary>
        private readonly Dictionary<string, SayDB> _databases = new Dictionary<string, SayDB>();

        /// <summary>
        ///     The main serializer
        /// </summary>
        internal DbSerializers Serializers { get; private set; }

        /// <summary>
        ///     Logger
        /// </summary>
        private readonly LogManager _logManager = new LogManager();

        public StoreTypeResolver TypeResolver { get; } = new StoreTypeResolver();

        internal DbWorkSpaceCore()
        {
            Activate();
        }

        public IPlatformAdapter PlatformAdapter { get; } = new PlatformAdapter();

        public LogManager LogManager
        {
            get { return _logManager; }
        }

        private static readonly Guid _databaseVersion = new Guid("0F82F535-ADFF-4007-9295-6A5D9B88590E");

        /// <summary>
        ///     The type dictating which objects should be ignored
        /// </summary>
        public virtual Type IgnoreAttribute { get { return typeof(DbIgnoreAttribute); } }

        /// <summary>
        ///     Back up the database
        /// </summary>
        /// <typeparam name="T">The database type</typeparam>
        /// <param name="writer">A writer to receive the backup</param>
        public async Task BackupAsync<T>(string dbName, BinaryWriter writer) where T : SayDB
        {
            _RequiresActivation();

            var database = GetDatabase(dbName);

            await database.FlushAsync().ConfigureAwait(false);

            // first write the version
            Serializers.Serialize(_databaseVersion, writer);

            var typeMaster = await database.Driver.GetTypesAsync().ConfigureAwait(false);

            // now the type master
            writer.Write(typeMaster.Count);

            foreach (var type in typeMaster)
            {
                writer.Write(type);
            }

            // now iterate tables
            foreach (var table in database.Core.StoreDefinitions)
            {
                // get the key list
                var keys = await database.Driver.DeserializeKeysAsync(table.Key, table.Value.KeyType, table.Value.GetNewDictionary()).ConfigureAwait(false);

                // reality check
                if (keys == null)
                {
                    writer.Write(0);
                }
                else
                {
                    // write the count for the keys
                    writer.Write(keys.Count);

                    // for each key, serialize it out along with the object - indexes  can be rebuilt on the flipside
                    foreach (var key in keys.Keys)
                    {
                        Serializers.Serialize(key, writer);
                        writer.Write((int)keys[key]);

                        // get the instance 
                        using (var instance = await database.Driver.LoadAsync(table.Key, (int)keys[key]).ConfigureAwait(false))
                        {
                            var bytes = instance.ReadBytes((int)instance.BaseStream.Length);
                            writer.Write(bytes.Length);
                            writer.Write(bytes);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Restore the database
        /// </summary>
        /// <typeparam name="T">Type of the database</typeparam>
        /// <param name="reader">The reader with the backup information</param>
        public async Task RestoreAsync<T>(string dbName, BinaryReader reader) where T : SayDB
        {
            _RequiresActivation();

            var database = GetDatabase(dbName);

            await database.PurgeAsync().ConfigureAwait(false);

            // read the version
            var version = Serializers.Deserialize<Guid>(reader);

            if (!version.Equals(_databaseVersion))
            {
                throw new SayDBException(string.Format("Unexpected database version."));
            }

            var typeMaster = new List<string>();

            var count = reader.ReadInt32();

            for (var x = 0; x < count; x++)
            {
                typeMaster.Add(reader.ReadString());
            }

            await database.Driver.DeserializeTypesAsync(typeMaster).ConfigureAwait(false);

            foreach (var table in database.Core.StoreDefinitions)
            {
                // make the dictionary 
                var keyDictionary = table.Value.GetNewDictionary();

                if (keyDictionary == null)
                {
                    throw new SayDBException(string.Format("Unable to make dictionary for key type {0}", table.Value.KeyType));
                }

                var keyCount = reader.ReadInt32();

                for (var record = 0; record < keyCount; record++)
                {
                    var key = Serializers.Deserialize(table.Value.KeyType, reader);
                    var keyIndex = reader.ReadInt32();
                    keyDictionary.Add(key, keyIndex);

                    var size = reader.ReadInt32();
                    var bytes = reader.ReadBytes(size);

                    await database.Driver.SaveAsync(table.Key, keyIndex, bytes).ConfigureAwait(false);
                }

                await database.Driver.SerializeKeysAsync(table.Key, table.Value.KeyType, keyDictionary).ConfigureAwait(false);

                // now refresh the table
                await table.Value.RefreshAsync().ConfigureAwait(false);

                // now generate the indexes 
                if (table.Value.Indices.Count <= 0) continue;

                var table1 = table;

                foreach (var key in keyDictionary.Keys)
                {
                    var instance = await database.LoadAsync(table1.Key, key).ConfigureAwait(false);

                    foreach (var index in table.Value.Indices)
                    {
                        await index.Value.AddIndexAsync(instance, key).ConfigureAwait(false);
                    }
                }

                foreach (var index in table.Value.Indices)
                {
                    await index.Value.FlushAsync().ConfigureAwait(false);
                }
            }
        }




        /// <summary>
        ///     Register a database type with the system
        /// </summary>
        /// <typeparam name="T">The type of the database to register</typeparam>
        public SayDB CreateDatabase(string instanceName, DbStorageType storageType = DbStorageType.Memory, string dataFolder = null)
        {
            try
            {
                _RequiresActivation();
                _logManager.Log(DbLogLevel.Information,
                    string.Format($"SayDB is registering database {instanceName}"),
                    null);

                if (_databases.ContainsKey(instanceName))
                    return _databases[instanceName];

                DbDriver driver = storageType == DbStorageType.Memory ? (DbDriver)new MemoryDriver() : new FileSystemDriver(dataFolder);

                driver.DatabaseInstanceName = instanceName;
                driver.DatabaseSerializer = Serializers;
                driver.Log = _logManager.Log;

                var dbCore = new DbCore(driver);

                var database = dbCore.CreateDatabase();

                _databases.Add(instanceName, database);

                return database;

            }
            catch (Exception ex)
            {
                throw new SayDbCreationException($"DB instance name='{instanceName}', storage type='{storageType}', data folder='{dataFolder}' ", ex);
            }
        }

        /// <summary>
        ///     Unloads/flushes the database instances
        /// </summary>
        private void _Unload()
        {
            foreach (var database in _databases.Values)
            {
                database.Unload();
            }
        }

        /// <summary>
        ///     Retrieve the database with the name
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <returns>The database instance</returns>
        public SayDB GetDatabase(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName");
            }

            _RequiresActivation();

            if (!_databases.ContainsKey(databaseName))
            {
                throw new SayDBDatabaseNotFoundException(databaseName);
            }

            return _databases[databaseName];
        }


        /// <summary>
        /// Register a class responsible for type resolution.
        /// </summary>
        /// <param name="typeResolver">The typeResolver</param>
        public void RegisterTypeResolver(IDbTypeResolver typeResolver)
        {
            TypeResolver.RegisterTypeResolver(typeResolver);
        }

        /// <summary>
        ///     Must be called to activate the engine. 
        ///     Can only be called once.
        /// </summary>
        private void Activate()
        {
            lock (_lock)
            {
                if (!_activated)
                {
                    Serializers = new DbSerializers(PlatformAdapter);
                    _activated = true;
                }
            }
        }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        internal void Deactivate()
        {
            lock (_lock)
            {
                if (_activated)
                {
                    _activated = false;
                    _Unload();
                    _databases.Clear();
                    //_serializer = new AggregateSerializer( this.PlatformAdapter );
                }
            }
        }

        /// <summary>
        ///     Requires that SayDB is activated
        /// </summary>
        private void _RequiresActivation()
        {
            if (!_activated)
            {
                throw new SayDBNotReadyException();
            }
        }

        protected virtual void Dispose(bool any = true) => Deactivate();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
