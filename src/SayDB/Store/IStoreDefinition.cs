using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Say32.DB.Core.Indexes;
using Say32.DB.Core.Keys;
using Say32.DB.Driver;

namespace Say32.DB.Store
{
    /// <summary>
    ///     Table definnition
    /// </summary>
    public interface IStoreDefinition
    {
        DataStoreID ID { get; }

        /// <summary>
        ///     Key list
        /// </summary>
        IKeyCollection Keys { get; }

        /// <summary>
        ///     Get a new dictionary (creates the generic)
        /// </summary>
        /// <returns>The new dictionary instance</returns>
        IDictionary GetNewDictionary();

        /// <summary>
        ///     Indexes
        /// </summary>
        Dictionary<string, IIndexCollection> Indices { get; }

        /// <summary>
        ///     Table type
        /// </summary>
        Type ObjectType { get; }

        /// <summary>
        ///     Key type
        /// </summary>
        Type KeyType { get; }

        /// <summary>
        ///     Refresh key list
        /// </summary>
        Task RefreshAsync();

        /// <summary>
        ///     Fetch the key for the instance
        /// </summary>
        /// <param name="instance">The instance</param>
        /// <returns>The key</returns>
        object GetKey(object instance);

        /// <summary>
        ///     Is the instance dirty?
        /// </summary>
        /// <returns>True if dirty</returns>
        bool IsDirty(object instance);
        
    }
}