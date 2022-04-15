using Say32.DB.Core;
using Say32.DB.Core.Events;
using Say32.DB.Core.Exceptions;
using Say32.DB.Object.Serialization;
using Say32.DB.Store;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB.Store.IO
{
    internal class StoreIo_LoadJob
    {
        private readonly DataStore _store;
        private readonly IoContext _context;

        public StoreIo_LoadJob(DataStore store, IoContext ioContext)
        {
            _store = store;
            _context = ioContext;
        }

        internal async Task<object> Start(object key)
        {
            var storeID = _store.ID;

            //Debug.WriteLine($"[[[DB.LOAD]]]] {store.ID}, {key}");
            Debug.Indent();

            var keyIndex = _store.IDef.Keys.GetIndexForKeyAsync(key);

            if (keyIndex < 0)
            {
                return null; // default( TResult );
            }

            if (_context.Cache.IsObjectCached(storeID, key))
            {
                var cached = _context.Cache.GetCachedObject(storeID, key);
                //Debug.WriteLine($"[DB.OBJECT CACHED ALRESDY] {cached}");
                return cached;
            }

            var obj = await LoadFromStore(storeID, key, keyIndex, _context);

            _store.DbCore.RaiseOperationEvent(DbOperation.Load, storeID, key);

            //Debug.WriteLine($"[DB.LOADED OBJECT] {obj}");

            Debug.Unindent();
            //Debug.WriteLine($"[[[DB.LOAD DONE]]] {storeID}, {key}");

            return obj;

        }

        private async Task<object> LoadFromStore(DataStoreID storeID, object key, int keyIndex, IoContext context)
        {
            BinaryReader br = null;
            MemoryStream memStream = null;

            try
            {
                br = await LoadMemoryStreamFromDriver(storeID, keyIndex, br);

                CustomizeLoadedByteStream(ref br, ref memStream);

                return await _store.DbObjectType.DeserializeObject(context, br, storeID, key, null);
            }
            finally
            {
                if (br != null)
                {
                    br.Dispose();
                }

                if (memStream != null)
                {
                    memStream.Flush();
                    memStream.Dispose();
                }
            }

        }

        private async Task<BinaryReader> LoadMemoryStreamFromDriver(DataStoreID storeID, int keyIndex, BinaryReader br) =>
            // open stream from driver
            await _store.DbCore.Driver.LoadAsync(storeID, keyIndex).ConfigureAwait(false);

        private void CustomizeLoadedByteStream(ref BinaryReader br, ref MemoryStream memStream)
        {
            if (_store.DbCore.Triggers.byteInterceptorList.Count > 0)
            {
                var bytes = br.ReadBytes((int)br.BaseStream.Length);

                bytes = _store.DbCore.Triggers.byteInterceptorList.ToArray().Reverse().Aggregate(bytes,
                                                                            (current, byteInterceptor) =>
                                                                            byteInterceptor.Load(current));

                memStream = new MemoryStream(bytes);

                br.Dispose();

                br = new BinaryReader(memStream);
            }
        }
    }
}
