using Say32.DB.Core;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using System.Threading.Tasks;

namespace Say32.DB.Store.IO
{
    internal class StoreIoJob
    {
        private readonly DataStore _store;
        private readonly IoContext _context;
        private readonly bool _isRootJob;

        private DbCore Db => _store.DbCore;

        public StoreIoJob(DataStore store)
        {
            _store = store;
        }

        public StoreIoJob(DataStore store, IoContext ioContext)
        {
            _store = store;
            _isRootJob = ioContext == null;
            _context = ioContext ?? _store.DbCore.CreateIoContext();
        }

        internal async Task<T> Load<T>(object key) where T : class
        {
            // need to lock store for loading???

            _store.TaskCounter.Increase();

            try
            {
                var loadJob = new StoreIo_LoadJob(_store, _context);
                T obj = (T)await loadJob.Start(key).ConfigureAwait(false) ?? default;

                if (_isRootJob)
                {
                    await _context.AwaitAllTasks();
                }

                Db.RaiseOperationEvent(DbOperation.Load, _store.ID, key);

                return obj;

            }
            finally
            {
                _store.TaskCounter.Decrease();
            }

        }


        internal async Task<TKey> Save<TKey>(object instance)
        {
            _store.TaskCounter.Increase();

            try
            {
                var saveJob = new StoreIo_SaveJob(_store);
                TKey key = (TKey)await saveJob.Start(instance, _context).ConfigureAwait(false);

                if (_isRootJob)
                {
                    await _context.AwaitAllTasks();
                }

                Db.RaiseOperationEvent(DbOperation.Save, _store.ID, key);

                return key;
            }
            finally
            {
                _store.TaskCounter.Decrease();
            }

        }

        public async Task DeleteAllAsync()
        {
            if (_store.TaskCounter.Count > 1)
            {
                throw new SayDBException(
                    ExceptionMessageFormat.BaseDatabaseInstance_Truncate_Cannot_truncate_when_background_operations);
            }

            _store.TaskCounter.Increase();

            try
            {
                var storeID = _store.ID;
                var storeDef = _store.IDef;

                await Db.Driver.TruncateAsync(storeID).ConfigureAwait(false);

                await storeDef.Keys.TruncateAsync().ConfigureAwait(false);

                foreach (var index in storeDef.Indices.Values)
                {
                    await index.TruncateAsync().ConfigureAwait(false);
                }

                Db.RaiseOperationEvent(DbOperation.Truncate, storeID, null);
            }
            finally
            {
                _store.TaskCounter.Decrease();
            }

        }
    }
}
