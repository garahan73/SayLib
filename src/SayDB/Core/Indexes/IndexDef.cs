using Say32.DB.Store.IO;
using System;
using System.Threading.Tasks;

namespace Say32.DB.Core.Indexes
{
    /// <summary>
    ///     An individual table key
    /// </summary>
    /// <typeparam name="T">The class the key maps to</typeparam>
    /// <typeparam name="TIndex">The type of the index</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public class IndexDef<T, TIndex, TKey> where T : class //, new() 
    {
        protected Func<TKey, IoContext, Task<T>> GetObjectFunc { get; private set; }
        private readonly int _hashCode;

        public TKey Key { get; private set; }

        /// <summary>
        ///     Key
        /// </summary>
        public TIndex Index { get; internal set; }

        //// <param name="getter">The getter</param>
        /// <summary>
        ///     Construct with how to get the key
        /// </summary>
        /// <param name="index">The index value</param>
        /// <param name="key">The associated key with the index</param>
        /// <param name="getter">Getter method for loading an instance</param>
        internal IndexDef(TIndex index, TKey key, Func<TKey, IoContext, Task<T>> getter)
        {
            Index = index;
            Key = key;
            _hashCode = key.GetHashCode();
            GetObjectFunc = getter;
        }

        /// <summary>
        ///     Compares for equality
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == _hashCode && ((IndexDef<T, TIndex, TKey>)obj).Key.Equals(Key);
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
            return string.Format("Index: [{0}][{1}]={2}", typeof(T).FullName, typeof(TIndex).FullName, Index);
        }

        public DbIndex<T, TIndex, TKey> CreateIndex(IoContext context)
            => new DbIndex<T, TIndex, TKey>(Index, Key, GetObjectFunc, context);
    }
}
