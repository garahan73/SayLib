using Say32.DB.Store.IO;
using System;
using System.Threading.Tasks;

namespace Say32.DB.Core.Keys
{
    /// <summary>
    ///     An individual table key
    /// </summary>
    /// <typeparam name="T">The class the key maps to</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    public class DbKey<T,TKey>: DbKeyDef<T,TKey> where T: class //, new()
    {
        private readonly IoContext _context;
        private Lazy<Task<T>> _lazyValue;

        public Task<T> Value => _lazyValue.Value;

        /// <summary>
        ///     Construct with how to get the key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="getter">The getter</param>
        public DbKey(TKey key, Func<TKey, IoContext, Task<T>> getter, IoContext context) :base(key, getter)
        {
            _context = context;
            _lazyValue = new Lazy<Task<T>>( async () => await LoadObjectAsync(Key, _context) );
        }

    }
}
