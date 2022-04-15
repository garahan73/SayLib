using System;
using System.Collections.Generic;
using System.IO;
using Say32.DB.Core;
using Say32.DB.Store;
using Say32.DB.Driver;
using Say32.DB.Log;

namespace Say32.DB.Server.FileSystem
{
    /// <summary>
    ///     Path provider
    /// </summary>
    public class PathProvider
    {  
        private const string TABLEMASTER = "TableMaster";
        private const string SayDB_ROOT = "SayDB Database";

        public const string TYPE = "types.dat";
        public const string KEY = "keys.dat";

        public static readonly string DefaultRootPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SayDB_ROOT);

        public string RootPath = DefaultRootPath;

        /// <summary>
        ///     Master index of tables 
        /// </summary>
        private readonly Dictionary<int, Dictionary<Type, int>> _tableMaster =
            new Dictionary<int, Dictionary<Type, int>>();

        /// <summary>
        ///     Validate base path
        /// </summary>
        /// <param name="basePath"></param>
        private static void _ContractForBasePath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentNullException("basePath");
            }

            if (!basePath.EndsWith(@"/"))
            {
                throw new ArgumentOutOfRangeException("basePath");
            }
        }

        /// <summary>
        ///     Validate database
        /// </summary>
        /// <param name="databaseName">The database name</param>
        private static void _ContractForDatabaseName(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName");
            }
        }

        /// <summary>
        ///     Contract for driver
        /// </summary>
        /// <param name="driver">The driver</param>
        private static void _ContractForDriver(DbDriver driver)
        {
            if (driver == null)
            {
                throw new ArgumentNullException("driver");
            }
        }

        /// <summary>
        ///     Contract for table type
        /// </summary>
        /// <param name="id">The table type</param>
        private static void _ContractForTableType(DataStoreID id)
        {
            if (id == null)
            {
                throw new ArgumentException("tableType");
            }
        }

        /// <summary>
        ///     Contract for table type
        /// </summary>
        /// <param name="indexName">The index name</param>
        private static void _ContractForIndexName(string indexName)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentException("indexName");
            }
        }
        
        /// <summary>
        ///     Get the path for a database
        /// </summary>
        /// <param name="basePath">The base path</param>
        /// <param name="databaseName">The database name</param>
        /// <param name="driver">The driver</param>
        /// <returns>The path</returns>
        public string GetDatabasePath(string basePath, string databaseName, DbDriver driver)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForDriver(driver);
            
            driver.Log(DbLogLevel.Verbose,
                            string.Format("Path Provider: Database Path Request: {0}", databaseName), null);

            var path = Path.Combine(RootPath, basePath, databaseName) + "/";

            driver.Log(DbLogLevel.Verbose, string.Format("Resolved database path from {0} to {1}",
                                                                    databaseName, path), null);
            return path;
        }

        /// <summary>
        ///     Generic table path
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="id"></param>
        /// <param name="driver"></param>
        /// <returns>The table path</returns>
        public string GetTablePath(string basePath, string databaseName, DataStoreID id, DbDriver driver)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForTableType(id);
            _ContractForDriver(driver);

            driver.Log(DbLogLevel.Verbose,
                            string.Format("Path Provider: Table Path Request: {0}", id), null);

            var path = Path.Combine(GetDatabasePath(basePath, databaseName, driver),
                                id.KeyName) + "/";

            driver.Log(DbLogLevel.Verbose, string.Format("Resolved table path from {0} to {1}",
                                                                    id, path), null);
            return path;
        }

        public string GetIndexPath(string basePath, string databaseName, DataStoreID id, DbDriver driver, string indexName)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForTableType(id);
            _ContractForDriver(driver);
            _ContractForIndexName(indexName);
            return Path.Combine(GetTablePath(basePath, databaseName, id, driver), string.Format("{0}.idx", indexName));
        }

        /// <summary>
        ///     Gets the folder to a specific instance
        /// </summary>
        /// <remarks>
        ///     Iso slows when there are many files in a given folder, so this allows
        ///     for partitioning of folders
        /// </remarks>
        /// <param name="basePath">Base path</param>
        /// <param name="databaseName">The database</param>
        /// <param name="id">The type of the table</param>
        /// <param name="driver">The driver</param>
        /// <param name="keyIndex">The key index</param>
        /// <returns>The path</returns>
        public string GetInstanceFolder(string basePath, string databaseName, DataStoreID id, DbDriver driver, int keyIndex)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForTableType(id);
            _ContractForDriver(driver);
            return Path.Combine(GetTablePath(basePath, databaseName, id, driver), (keyIndex/100).ToString()) + "\\";                
        }

        /// <summary>
        ///     Gets the path to a specific instance
        /// </summary>
        /// <param name="basePath">Base path</param>
        /// <param name="databaseName">The database</param>
        /// <param name="id">The type of the table</param>
        /// <param name="driver">The driver</param>
        /// <param name="keyIndex">The key index</param>
        /// <returns>The path</returns>
        public string GetInstancePath(string basePath, string databaseName, DataStoreID id, DbDriver driver, int keyIndex)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForTableType(id);
            _ContractForDriver(driver);
            return Path.Combine(GetInstanceFolder(basePath, databaseName, id, driver, keyIndex), keyIndex.ToString());
        }

        /// <summary>
        ///     Get keys path
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="databaseName"></param>
        /// <param name="id"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public string GetKeysPath(string basePath, string databaseName, DataStoreID id, DbDriver driver)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForTableType(id);
            _ContractForDriver(driver);
            return Path.Combine(GetTablePath(basePath, databaseName, id, driver), KEY);
        }

        /// <summary>
        ///     Get types path
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="databaseName"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public string GetTypesPath(string basePath, string databaseName, DbDriver driver)
        {
            _ContractForBasePath(basePath);
            _ContractForDatabaseName(databaseName);
            _ContractForDriver(driver);
            return Path.Combine(GetDatabasePath(basePath, databaseName, driver), TYPE);
        }  
    }
}