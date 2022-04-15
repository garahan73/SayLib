using Say32.DB;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using Say32.DB.Object.Serialization;
using Say32.DB.Store;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB.Store.IO
{
    internal class StoreIo_SaveJob
    {
        private readonly DataStore _store;
        private IoContext _context;

        public StoreIo_SaveJob(DataStore store)
        {
            _store = store;
        }

        internal async Task<object> Start(object instance, IoContext ioContext)
        {
            _context = ioContext;

            var storeID = _store.ID;

            //Debug.WriteLine($"[[[DB.SAVE]]] {storeID}");
            Debug.Indent();

            //Debug.WriteLine($"[object] {instance}");

            IStoreDefinition storeDef = null;

            try
            {
                var dbCore = _context.DbCore;

                storeDef = _store.IDef;

                var type = instance?.GetType() ?? storeDef.ObjectType;

                //Debug.WriteLine($"[type] {type.Name}");

                var key = storeDef.GetKey(instance);
                //Debug.WriteLine($"[key] {key}");

                if (!storeDef.IsDirty(instance))
                    return key;

                CustomizeBeforeSave(instance, storeID, dbCore, type);

                // check if already cached
                if (IsCached_Or_CacheIfNeeded(key, instance))
                    return key;

                var keyIndex = await storeDef.Keys.AddKeyAsync(key).ConfigureAwait(false);

                await SaveToStore(instance, keyIndex);

                await _store.UpdateIndexes(instance, key);

                // call post-save triggers
                CustomizeAfterSave(instance, storeID, dbCore, type);

                dbCore.RaiseOperationEvent(DbOperation.Save, storeID, key);

                return key;
            }
            finally
            {
                //Debug.WriteLine($"[saved object] {instance}");
                Debug.Unindent();
                //Debug.WriteLine($"[[[DB.SAVE DONE]]] {storeID}");
            }
        }



        private bool IsCached_Or_CacheIfNeeded(object key, object instance)
        {
            if (_context.Cache.IsObjectCached(_store.ID, key))
            {
                //Debug.WriteLine($"[Saving object cached] object is already cached to save. store={storeID}, key={key}");
                return true;
            }
            else // cache if not cached
            {
                _context.Cache.CacheObject(_store.ID, key, instance);
                return false;
            }
        }

        private async Task SaveToStore(object instance, int keyIndex)
        {
            var memStream = new MemoryStream();

            try
            {
                using (var bw = new BinaryWriter(memStream))
                {
                    await _store.DbObjectType.SerializeObject(_context, instance, bw, null);
                    bw.Flush();

                    memStream = CustomizeMemStream(memStream);

                    await _context.DbCore.Driver.SaveAsync(_store.ID, keyIndex, memStream.ToArray()).ConfigureAwait(false);
                }
            }
            finally
            {
                memStream.Flush();
                memStream.Dispose();
            }
        }

        private MemoryStream CustomizeMemStream(MemoryStream memStream)
        {
            var dbCore = _context.DbCore;

            if (dbCore.Triggers.byteInterceptorList.Count > 0)
            {
                var bytes = memStream.ToArray();

                bytes = dbCore.Triggers.byteInterceptorList.Aggregate(bytes,
                                                        (current, byteInterceptor) =>
                                                        byteInterceptor.Save(current));

                memStream = new MemoryStream(bytes);
            }

            return memStream;
        }

        private static void CustomizeBeforeSave(object instance, DataStoreID storeID, Core.DbCore dbCore, Type type)
        {
            foreach (var trigger in dbCore.Triggers.GetStoreTriggers(storeID).Where(trigger => !trigger.BeforeSave(type, instance)))
            {
                throw new SayDBTriggerException(ExceptionMessageFormat.BaseDatabaseInstance_Save_Save_suppressed_by_trigger, trigger.GetType());
            }
        }

        private static void CustomizeAfterSave(object instance, DataStoreID storeID, Core.DbCore dbCore, Type type)
        {
            foreach (var trigger in dbCore.Triggers.GetStoreTriggers(storeID))
            {
                trigger.AfterSave(type, instance);
            }
        }


    }
}
