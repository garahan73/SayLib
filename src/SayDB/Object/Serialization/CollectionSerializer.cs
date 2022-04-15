using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Say32.DB.Object.Serialization
{


    class CollectionSerializer
    {
        private readonly IoContext _ctx;
        private BinaryWriter _bw;
        private DataStoreID _storeID;

        public CollectionSerializer(IoContext ctx, BinaryWriter bw, DataStoreID storeID)
        {
            _ctx = ctx;
            _bw = bw;
            _storeID = storeID;
        }

        public async Task SerializeArray(IoHeader header, Array array)
        {
            header.IsArray = true;
            header.Serialize(_bw);

            _bw.Write(array.Length);

            //var i = 0;
            foreach (var item in array)
            {
                //_bw.Write(i++);
                await _SerializeCollectionItem(item);

                Debug.Indent();
                //Debug.WriteLine($"<<<<<<ARRAY ITEM>>>>>> {item}");
                Debug.Unindent();

            }
        }

        public async Task SerializeList(IoHeader header, IList list)
        {
            header.IsList = true;
            header.Serialize(_bw);

            _bw.Write(list.Count);

            //var i = 0;
            foreach (var item in list)
            {
                //_bw.Write(i++);
                await _SerializeCollectionItem(item);

                Debug.Indent();
                //Debug.WriteLine($"<<<<<<LIST ITEM>>>>>> {item}");
                Debug.Unindent();
            }
        }

        public async Task SerializeDictionary(IoHeader header, IDictionary dictionary)
        {
            //Debug.WriteLine($"[Save dictionary] count={dictionary?.Count}");
            Debug.Indent();

            header.IsDictionary = true;
            header.Serialize(_bw);

            _bw.Write(dictionary.Count);

            foreach (var item in dictionary.Keys)
            {
                await _SerializeCollectionItem(item);
                await _SerializeCollectionItem(dictionary[item]);

                Debug.Indent();
                //Debug.WriteLine($"<<<<<<DIC. ITEM>>>>>> [{item}]={dictionary[item]}");
                Debug.Unindent();

            }
            Debug.Unindent();
        }
        private async Task _SerializeCollectionItem(object item)
        {
            var dbObjType = _ctx.DbCore.DbObjectTypeManager.GetDbObjectType(item.GetType(), _storeID);
            var header = new IoHeader { TypeCode = await GetTypeCode(dbObjType.ObjectType) };
            await dbObjType.SerializePropertyValue(item, _ctx, _bw, header);
        }

        private async Task<int> GetTypeCode(Type type)
        {
            return await _ctx.TypeManager.GetTypeIndex(type.AssemblyQualifiedName);
        }
    }
}
