using Say32.DB.Core;
using Say32.DB.Core.Async;
using Say32.DB.Core.Serialization;
using Say32.DB.Log;
using Say32.DB.Serialization;
using Say32.DB.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB
{
    /// <summary>
    ///     The SayDB database manager
    /// </summary>
    public static class DbWorkSpace
    {
        internal static DbWorkSpaceCore Core { get; set; }

        public static DbWorkSpaceLifeCycleDelegate Open()
        {
            Core?.Deactivate();
            Core = new DbWorkSpaceCore();

            return new DbWorkSpaceLifeCycleDelegate();
        }

        public static bool IsOpen => Core != null;

        public static IPlatformAdapter PlatformAdapter => Core?.PlatformAdapter;

        public static LogManager LogManager => Core?.LogManager;

        public static DbSerializers Serializers => Core?.Serializers;

        public static StoreTypeResolver TypeResolver => Core?.TypeResolver;

        /// <summary>
        ///     The type dictating which objects should be ignored
        /// </summary>
        public static Type IgnoreAttribute => Core?.IgnoreAttribute;

        public static string DataFolder { get; set; }

        /// <summary>
        ///     Back up the database
        /// </summary>
        /// <typeparam name="T">The database type</typeparam>
        /// <param name="writer">A writer to receive the backup</param>
        public static async Task BackupDatabaseAsync<T>(string dbName, BinaryWriter writer) where T : SayDB
            => await Core.BackupAsync<T>(dbName, writer).ConfigureAwait(false);

        /// <summary>
        ///     Restore the database
        /// </summary>
        /// <typeparam name="T">Type of the database</typeparam>
        /// <param name="reader">The reader with the backup information</param>
        public static async Task RestoreDatabaseAsync<T>(string dbName, BinaryReader reader) where T : SayDB
            => await Core?.RestoreAsync<T>(dbName, reader);//.ConfigureAwait(false);


        /// <summary>
        ///     Register a database type with the system
        /// </summary>
        /// <typeparam name="T">The type of the database to register</typeparam>
        public static SayDB CreateDatabase(string instanceName, DbStorageType storageType = DbStorageType.Memory, string dataFolderPath=null)
            => Core?.CreateDatabase(instanceName, storageType, dataFolderPath ?? DataFolder);

        /// <summary>
        ///     Retrieve the database with the name
        /// </summary>
        /// <param name="databaseName">The database name</param>
        /// <returns>The database instance</returns>
        public static SayDB GetDatabase(string databaseName) => Core?.GetDatabase(databaseName);

        /// <summary>
        /// Register a class responsible for type resolution.
        /// </summary>
        /// <param name="typeResolver">The typeResolver</param>
        public static void RegisterTypeResolver(IDbTypeResolver typeResolver) => Core?.RegisterTypeResolver(typeResolver);

        public static void Close() => Core?.Deactivate();

    }

    public class DbWorkSpaceLifeCycleDelegate : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool includeManaged)
        {
            DbWorkSpace.Core?.Deactivate();
            DbWorkSpace.Core = null;
        }
    }

    public static class SayDbExtensions
    {
        public static EnumerableTasksAwaiter<T> GetAwaiter<T>(this IEnumerable<Task<T>> tasks) => new EnumerableTasksAwaiter<T>(tasks);

        public static IEnumerable<R> WaitAllTasks<R>(this IEnumerable<Task<R>> tasks, int waitInMilliseconds = -1)
        {
            if (Task.WaitAll(tasks.ToArray(), waitInMilliseconds))
                return tasks.Select(t => t.Result);
            else
                throw new TimeoutException($"Failed to wait all tasks of array. timeout by {waitInMilliseconds} milliseconds");
        }

        public static R WaitAnyTask<R>(this IEnumerable<Task<R>> tasks, int waitInMilliseconds = -1)
        {
            var array = tasks.ToArray();
            var index = Task.WaitAny(array, waitInMilliseconds);
            if (index >= 0)
                return array[index].Result;

            throw new TimeoutException($"Failed to wait any one task of array. timeout by {waitInMilliseconds} milliseconds");
        }

    }
}
