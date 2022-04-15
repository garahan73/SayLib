using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{
    class CollectionDeserializer
    {
        private readonly IoContext _ctx;
        private BinaryReader _br;
        private DataStoreID _storeID;

        public CollectionDeserializer(IoContext ctx, BinaryReader br, DataStoreID storeID)
        {
            _ctx = ctx;
            _br = br;
            _storeID = storeID;
        }

        public void DeserializeArrayItems(Array array, int count)
        {
            //Debug.WriteLine($"[Load array items] count={count}");

            Debug.Indent();

            for (var x = 0; x < count; x++)
            {
                //Debug.WriteLine($"Array[{x}]");
                var task = DeserializeCollectionItem(value => array.SetValue(value, x));
                _ctx.Cache.CacheTask(task);
                //Debug.WriteLine($"<<<<<<ARRAY ITEM>>>>>> [{x}] = {value}");
            }

            Debug.Unindent();
        }

        public async Task DeserializeListItemsAsync(IList list)
        {
            var count = _br.ReadInt32();
            //Debug.WriteLine($"[Load list] count={count}");

            Debug.Indent();
            for (var x = 0; x < count; x++)
            {
                await DeserializeCollectionItem(v => list.Add(v));
            }

            Debug.Unindent();
        }


        public async Task DeserializeDictionaryItems(IDictionary dictionary)
        {
            var count = _br.ReadInt32();
            //Debug.WriteLine($"<<<<DICTIONARY>>>> size={count}");

            Debug.Indent();
            for (var x = 0; x < count; x++)
            {
                await DeserializeCollectionItem(async key =>
                {
                    await DeserializeCollectionItem(value => dictionary.Add(key, value));
                });
            }
            Debug.Unindent();
            //Debug.WriteLine($"<<<<DICTIONARY FINISHED>>>> size={count}");

        }

        private async Task DeserializeCollectionItem(SetPropertyValueMethod setPropValue)
        {
            try
            {
                var header = IoHeader.Deserialize(_br);
                var type = await _ctx.GetType(header);
                var storeID = header.HasStore ? _ctx.DbCore.StoreMap.GetByStoreKeyName(header.StoreKeyName) : null;

                var dbObjType = _ctx.DbCore.DbObjectTypeManager.GetDbObjectType(type, storeID);
                await dbObjType.DeserializePropertyValue(_ctx, _br, header, setPropValue);
            }
            catch (Exception ex)
            {
                throw new SayDbException("Failed to deserialize collection item", ex);
            }

        }
    }
}
