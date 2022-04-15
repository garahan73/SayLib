using System;
using System.Linq;
using System.Reflection;
using Say32.DB.Core.Serialization;
using Say32.DB.Store;

namespace Say32.DB.Core.Database
{
    /// <summary>
    ///     Extensions for the database
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Extension to register an index
        /// </summary>
        /// <typeparam name="T">The type of the table</typeparam>
        /// <typeparam name="TIndex">The index</typeparam>
        /// <typeparam name="TKey">The key</typeparam>
        /// <param name="table">The table definition</param>
        /// <param name="name">The name of the index</param>
        /// <param name="indexer">The indexer</param>
        /// <returns>The table</returns>
        public static IStoreDefinition WithIndex<T,TIndex,TKey>(this IStoreDefinition table, string name, Func<T,TIndex> indexer) where T: class, new()
        {
            ((StoreDefinition<T,TKey>)table).RegisterIndex(name, indexer);
            return table;
        }

        /// <summary>
        ///     Extension to register an index
        /// </summary>
        /// <typeparam name="T">The type of the table</typeparam>
        /// <typeparam name="TIndex1">The index</typeparam>
        /// <typeparam name="TIndex2">The second index</typeparam>        
        /// <typeparam name="TKey">The key</typeparam>
        /// <param name="table">The table definition</param>
        /// <param name="name">The name of the index</param>
        /// <param name="indexer">The indexer</param>
        /// <returns>The table</returns>
        public static IStoreDefinition WithIndex<T, TIndex1, TIndex2, TKey>(this IStoreDefinition table, string name, Func<T, Tuple<TIndex1,TIndex2>> indexer) 
            where T : class, new()
        {
            ((StoreDefinition<T, TKey>)table).RegisterIndex(name, indexer);
            return table;
        }

        /// <summary>
        ///     Extension to register the dirty flag
        /// </summary>
        /// <typeparam name="T">The type of the table</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>The table</returns>
        public static IStoreDefinition WithDirtyFlag<T,TKey>(this IStoreDefinition table, Predicate<T> isDirty)
            where T : class, new()
        {
            ((StoreDefinition<T, TKey>)table).RegisterDirtyFlag(isDirty);
            return table;
        }


        /// <summary>
        ///     Is a property ignored?
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsIgnored(this PropertyInfo p, Type ignoreAttribute)
        {
            return p.GetCustomAttributes(ignoreAttribute,false).Any();
        }

        /// <summary>
        ///     Is a property ignored?
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsIgnored(this FieldInfo f, Type ignoreAttribute)
        {
            return f.GetCustomAttributes(ignoreAttribute,false).Any();
        }

        /// <summary>
        ///     Is a property ignored?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIgnored(this Type type, Type ignoreAttribute, IPlatformAdapter platformAdapter)
        {
            return platformAdapter.GetCustomAttributes( type, ignoreAttribute,false).Any();
        }

    }
}
