using System;
using System.Collections;
using System.Collections.Generic;

namespace Say32
{
    public static class SayDictionaryUtil
    {
        public static void Merge<K, V>( this IDictionary<K, V> dic, IDictionary<K, V> toBeMerged )
        {
            toBeMerged.ForEach(kv => dic[kv.Key] = kv.Value);
        }

        public static Dictionary<K,V> ToDictionary<K,V>(this IDictionary<K,V> dic)
        {
            var r = new Dictionary<K, V>();
            dic.ForEach(kv => r.Add(kv.Key, kv.Value));

            return r;
        }

        public static Dictionary<K, V> Plus<K, V>( this IDictionary<K, V> dic1, IDictionary<K,V> dic2 )
        {
            var r = new Dictionary<K, V>(dic1);
            r.Merge(dic2);

            return r;
        }

        public static bool TryRemove<K,V>( this IDictionary<K, V> dic, K key, out V deleted)
        {
            if (dic.TryGetValue(key, out deleted))
            {
                dic.Remove(key);
                return true;
            }

            return false;
        }



    }

    public class ThreadSafeDictionary<K, V> : IDictionary<K, V>, IDictionary
    {
        private readonly IDictionary<K, V> _dic;
        protected readonly object _key = new object();

        public ThreadSafeDictionary() : this(new Dictionary<K,V>())
        {
        }

        public ThreadSafeDictionary( IDictionary<K, V> dic )
        {
            _dic = dic;
        }

        public virtual V this[K key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public V Get(K key)
        {
            lock (_key)
            {
                //Console.WriteLine($"get: CIM DATA[{key}] = {_dic[key]}"); 
                return _dic[key];
            }
        }

        public void Set(K key, V value)
        {
            lock (_key)
            {
                _dic[key] = value;
                //Console.WriteLine($"set: CIM DATA[{key}] = {_dic[key]}");
            }
        }

        public ICollection<K> Keys
        {
            get { lock (_key) { return _dic.Keys; } }
        }

        public ICollection<V> Values
        {
            get { lock (_key) { return _dic.Values; } }
        }

        public int Count
        {
            get { lock (_key) { return _dic.Count; } }
        }

        public bool IsReadOnly
        {
            get { lock (_key) { return _dic.IsReadOnly; } }
        }

        private IDictionary IDic => (IDictionary)_dic;

        public bool IsFixedSize => IDic.IsFixedSize;

        ICollection IDictionary.Keys
        {
            get
            {
                lock (_key) { return IDic.Keys; }
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                lock (_key) { return IDic.Values; }
            }
        }

        public bool IsSynchronized => IDic.IsSynchronized;

        public object SyncRoot => IDic.SyncRoot;

        public object this[object key] { get => IDic[key]; set => IDic[key] = value; }

        public virtual void Add( K key, V value )
        {
            lock (_dic) { _dic.Add(key, value); }
        }

        public virtual void Add( KeyValuePair<K, V> item )
        {
            lock (_key) { _dic.Add(item); }
        }

        public void Clear()
        {
            lock (_key) { _dic.Clear(); }
        }

        public bool Contains( KeyValuePair<K, V> item )
        {
            lock (_key) { return _dic.Contains(item); }
        }

        public bool ContainsKey( K key )
        {
            lock (_key) { return _dic.ContainsKey(key); }
        }

        public void CopyTo( KeyValuePair<K, V>[] array, int arrayIndex )
        {
            lock (_key) { _dic.CopyTo(array, arrayIndex); }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            lock (_key) { return Clone().GetEnumerator(); }
        }

        public bool Remove( K key )
        {
            lock (_key) { return _dic.Remove(key); }
        }

        public bool Remove( KeyValuePair<K, V> item )
        {
            lock (_key) { return _dic.Remove(item); }
        }

        public bool TryGetValue( K key, out V value )
        {
            lock (_key) { return _dic.TryGetValue(key, out value); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator(); 
        }

        public void Add( object key, object value )
        {
            lock(_key) { IDic.Add(key, value); }
        }

        public bool Contains( object key )
        {
            lock (_key) { return IDic.Contains(key); }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            lock (_key) { return Clone().GetEnumerator(); }
        }

        public void Remove( object key )
        {
            lock (_key) { IDic.Remove(key); }
        }

        public void CopyTo( Array array, int index )
        {
            lock (_key) { IDic.CopyTo(array, index); }
        }

        public Dictionary<K,V> Clone()
        {
            lock (_key)
            {
                var r = new Dictionary<K, V>();

                // should not call KeyValue pair enumerator
                foreach (var key in Keys)
                {
                    r.Add(key, this[key]);
                }
                return r;
            }
        }
    }
}
