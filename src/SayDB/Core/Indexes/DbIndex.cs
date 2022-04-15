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
    public class DbIndex<T, TIndex, TKey> : IndexDef<T, TIndex, TKey> where T : class
    {
        private Lazy<Task<T>> _lazyValue;
        private IoContext _context;

        //// <param name="getter">The getter</param>
        /// <summary>
        ///     Construct with how to get the key
        /// </summary>
        /// <param name="index">The index value</param>
        /// <param name="key">The associated key with the index</param>
        /// <param name="getter">Getter method for loading an instance</param>
        internal DbIndex(TIndex index, TKey key, Func<TKey, IoContext, Task<T>> getter, IoContext context)
            : base(index, key, getter)
        {
            _context = context;
            _lazyValue = new Lazy<Task<T>>(async () => await GetObjectFunc(Key, context));
        }

        public Task<T> Value => _lazyValue.Value;



    }
}
