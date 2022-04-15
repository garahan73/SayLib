using System;
using System.Threading.Tasks;
using Say32.DB.Store.IO;

namespace Say32.DB.Core.Keys
{
    /// <summary>
    ///     An individual table key
    /// </summary>
    /// <typeparam name="T">The class the key maps to</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public class DbKeyDef<T,TKey> where T: class //, new()
    {
        public TKey Key { get; private set; }


        protected Func<TKey, IoContext, Task<T>> LoadObjectAsync { get; private set; }
        private int _hashCode; 

        /// <summary>
        ///     Construct with how to get the key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="getter">The getter</param>
        public DbKeyDef(TKey key, Func<TKey, IoContext, Task<T>> getter)
        {
            Key = key;
            _hashCode = key.GetHashCode();
            LoadObjectAsync = getter;        
        }

        /// <summary>
        ///     Compares for equality
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == _hashCode && ((DbKeyDef<T, TKey>) obj).Key.Equals(Key);
        }

        /// <summary>
        ///     Hash code
        /// </summary>
        /// <returns>The has code of the key</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>The key</returns>
        public override string ToString()
        {
            return string.Format("Key: [{0}][{1}]={2}", typeof (T).FullName, typeof (TKey).FullName, Key);
        }

        public DbKey<T, TKey> CreateKey(IoContext context)
        {
            return new DbKey<T, TKey>(Key, LoadObjectAsync, context);
        }
    }
}
