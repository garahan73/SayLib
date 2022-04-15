using Say32.DB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Say32.DB.Core.TypeSupport
{
    class TypeManager
    {
        private Dictionary<int, Type> _typeMap = new Dictionary<int, Type>();
        private readonly Dictionary<string, int> _indexMap = new Dictionary<string, int>();

        private readonly DbDriver _driver;


        public TypeManager(DbDriver driver)
        {
            _driver = driver;
        }

        public async Task<Type> GetType(int index)
        {
            lock (this)
            {
                if (_typeMap.ContainsKey(index))
                    return _typeMap[index];
            }
            var typeName = await _driver.GetTypeAtIndexAsync(index);

            return Update(index, typeName);
        }


        public async Task<int> GetTypeIndex(string typeName)
        {
            lock (this)
            {
                if (_indexMap.ContainsKey(typeName))
                    return _indexMap[typeName];
            }

            var index = await _driver.GetTypeIndexAsync(typeName);
                        
            Update(index, typeName);

            return index;
        }

        private Type Update(int index, string typeName)
        {
            var type = Type.GetType(typeName);

            lock (this)
            {
                _typeMap.Add(index, type);
                _indexMap.Add(typeName, index);
            }

            return type;
        }

    }
}
