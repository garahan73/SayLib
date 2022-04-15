using Say32.DB.Core.Serialization;
using Say32.DB.Log;
using Say32.DB.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable CS1998

namespace Say32.DB.Driver
{
    /// <summary>
    ///     Base driver
    /// </summary>
    public abstract class DbDriver
    {
        protected List<string> TypeIndex { get; private set; }

        protected DbDriver()
        {
            TypeIndex = new List<string>();
#if DEBUG
            Log = (level, msg, ex) => Debug.WriteLine("Level: {0} Message: {1} Exception: {2}",
                                                         level,
                                                         msg ?? "<null>",
                                                         ex == null ? "<null>" : ex.GetType().FullName);
#else
            Log = ( level, msg, ex ) => { };
#endif
        }

        /// <summary>
        ///     Name of the database the driver is registered to
        /// </summary>
        public string DatabaseInstanceName { get; set; }

        /// <summary>
        ///     Logger
        /// </summary>
        public Action<DbLogLevel, string, Exception> Log { get; set; }

        /// <summary>
        ///     The registered serializer for the database
        /// </summary>
        public IDbSerializer DatabaseSerializer { get; set; }

        /// <summary>
        ///     Serialize the keys
        /// </summary>
        /// <param name="type">Type of the parent table</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="keyMap">Key map</param>
        public abstract Task SerializeKeysAsync(DataStoreID id, Type keyType, IDictionary keyMap);

        /// <summary>
        ///     Deserialize keys without generics
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="keyType">Type of the key</param>
        /// <param name="template">The template</param>
        /// <returns>The keys without the template</returns>
        public abstract Task<IDictionary> DeserializeKeysAsync(DataStoreID id, Type keyType, IDictionary template);

        /// <summary>
        ///     Serialize a single index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="id">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>
        public abstract Task SerializeIndexAsync<TKey, TIndex>(DataStoreID id, string indexName, Dictionary<TKey, TIndex> indexMap);

        /// <summary>
        ///     Serialize a double index 
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="id">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>
        /// <param name="indexMap">The index map</param>        
        public abstract Task SerializeIndexAsync<TKey, TIndex1, TIndex2>(DataStoreID id, string indexName,
                                                                    Dictionary<TKey, Tuple<TIndex1, TIndex2>> indexMap);

        /// <summary>
        ///     Deserialize a single index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex">The type of the index</typeparam>
        /// <param name="id">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>
        public abstract Task<Dictionary<TKey, TIndex>> DeserializeIndexAsync<TKey, TIndex>(DataStoreID id, string indexName);

        /// <summary>
        ///     Deserialize a double index
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TIndex1">The type of the first index</typeparam>
        /// <typeparam name="TIndex2">The type of the second index</typeparam>
        /// <param name="type">The type of the parent table</param>
        /// <param name="indexName">The name of the index</param>        
        /// <returns>The index map</returns>        
        public abstract Task<Dictionary<TKey, Tuple<TIndex1, TIndex2>>> DeserializeIndexAsync<TKey, TIndex1, TIndex2>(DataStoreID id,
                                                                                                           string
                                                                                                               indexName);

        /// <summary>
        ///     Publish the list of tables
        /// </summary>
        /// <param name="tables">The list of tables</param>
        public abstract void PublishDataStores(Dictionary<DataStoreID, IStoreDefinition> tables, Func<string, Type> resolveType);

        /// <summary>
        ///     Serialize the type master
        /// </summary>
        public abstract Task SerializeTypesAsync();

        /// <summary>
        ///     Deserialize the type master
        /// </summary>
        /// <param name="types">The list of types</param>
        public async Task DeserializeTypesAsync(IList<string> types)
        {
            TypeIndex = new List<string>(types);
        }

        /// <summary>
        ///     Get the type master
        /// </summary>
        /// <returns></returns>
        public Task<IList<string>> GetTypesAsync()
        {
            return Task.FromResult((IList<string>)new List<string>(TypeIndex));
        }

        /// <summary>
        ///     Get the index for the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The type</returns>
        public virtual Task<int> GetTypeIndexAsync(string type)
        {
            return Task.Factory.StartNew(() =>
           {
               lock (((ICollection)TypeIndex).SyncRoot)
               {
                   if (!TypeIndex.Contains(type))
                   {
                       TypeIndex.Add(type);
                   }
                   return TypeIndex.IndexOf(type);
               }
           });
        }

        /// <summary>
        ///     Get the type at an index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The type</returns>
        public virtual Task<string> GetTypeAtIndexAsync(int index)
        {
            return Task.FromResult(TypeIndex[index]);
        }

        /// <summary>
        ///     Save operation
        /// </summary>
        /// <param name="type">Type of the parent</param>
        /// <param name="keyIndex">Index for the key</param>
        /// <param name="bytes">The byte stream</param>
        public abstract Task SaveAsync(DataStoreID tableID, int keyIndex, byte[] bytes);

        /// <summary>
        ///     Load from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        /// <returns>The byte stream</returns>
        public abstract Task<BinaryReader> LoadAsync(DataStoreID tableID, int keyIndex);

        /// <summary>
        ///     Delete from the store
        /// </summary>
        /// <param name="type">The type of the parent</param>
        /// <param name="keyIndex">The index of the key</param>
        public abstract Task DeleteAsync(DataStoreID id, int keyIndex);

        /// <summary>
        ///     Truncate a type
        /// </summary>
        /// <param name="type">The type to truncate</param>
        public abstract Task TruncateAsync(DataStoreID id);

        /// <summary>
        ///     Purge the database
        /// </summary>
        public abstract Task PurgeAsync();

    }
}