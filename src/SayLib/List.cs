using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{

    public static class SayListUtil
    {
        public static void Add<T>(this List<T> list, params T[] items)
        {
            list.AddRange(items);
        }

        public static T Last<T>( this List<T> list ) => list[list.Count - 1];
        public static T LastOrDefault<T>( this List<T> list ) => list.Count == 0 ? default : list[list.Count - 1];

        public static int Replace<T>(this List<T> list, T original, T newOne)
        {
            var index = list.IndexOf(original);
            if (index < 0)
                throw new InvalidOperationException($"Can't replace in the list. Original item doesn't exists");

            list[index] = newOne;

            return index;
        }

        public static bool AreEqualsTo(this IEnumerable list, IEnumerable items)
        {
            var list_ = list.Cast<object>().ToArray();
            var items_ = items.Cast<object>().ToArray();

            if (list_.Length != items_.Length)
                return false;

            for (int i = 0; i < list_.Length; i++)
            {
                if (!Equals(list_[i], items_[i]))
                    return false;
            }

            return true;
        }

        public static bool AreEqualsTo( this IEnumerable list, params object[] items )
        {
            return AreEqualsTo(list, (IEnumerable)items);
        }

        public static T GetSingleItem<T>( this IList<T> list )
        {
            if (list.Count != 1)
                throw new ListSingleItemException($"Count = {list.Count}");

            return list[0];
        }

        public static object GetSingleItemObject( this IList list )
        {
            if (list.Count != 1)
                throw new ListSingleItemException($"Count = {list.Count}");

            return list[0];
        }



        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }


    }

    public class SayList<D> : List<D>
    {        
        public SayList()
        {
        }

        public SayList(params D[] items) : base(items)
        {
        }


        public SayList(IEnumerable<D> items) : base(items)
        {
        }

        public SayList(int capacity) : base(capacity)
        {
        }

        public bool IsEmpty => Count == 0;

        public static SayList<D> operator +(SayList<D> list1, IEnumerable<D> list2)
        {
            return new SayList<D>(list1.Concat(list2));
        }

        public static SayList<D> operator +(SayList<D> list, D item)
        {
            var r = new SayList<D>(list);
            r.Add(item);
            return r;
        }

        public static SayList<D> operator -(SayList<D> list, IList<D> list2)
        {
            var r = new SayList<D>(list);
            foreach (var item in list2)
            {
                r.Remove(item);
            }
            return r;
        }

        public static SayList<D> operator -(SayList<D> list, D item)
        {
            var r = new SayList<D>(list);
            r.Remove(item);
            return r;
        }

        //public static implicit operator SayList<D>(List<D> list)=> new SayList<D>(list);


        public SayList<D> Clone()
        {
            var r = new SayList<D>();

            foreach (var item in this)
            {
                if (item is ICloneable src)
                {
                    r.Add((D)src.Clone());
                }
                else
                {
                    r.Add(item);
                }
            }
            return r;
        }

        public override bool Equals( object obj )
        {
            if (!(obj is IList<D> list))
                return false;

            if (Count != list.Count) return false;

            for (int i = 0; i < Count; i++)
            {
                if (!Equals(this[i], list[i]))
                    return false;
            }

            return true;
        }

        public static bool operator ==( SayList<D> a, SayList<D> b ) => Equals(a, b);

        public static bool operator !=(SayList<D> a, SayList<D> b) => !Equals(a, b);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString() => this.ToStringExt();
    }

    public class ThreadSafeList<T> : IList<T>
    {
        protected readonly IList<T> _list;

        protected static object _key = new object();

        public ThreadSafeList()
        {
            _list = new List<T>();
        }

        public ThreadSafeList(int size, bool initWithDefaultValues=false)
        {
            _list = new List<T>(size);

            if (initWithDefaultValues)
            {
                for (int i = 0; i < size; i++)
                {
#pragma warning disable CS8604 // 가능한 null 참조 인수입니다.
                    _list.Add(default);
#pragma warning restore CS8604 // 가능한 null 참조 인수입니다.
                }
            }

        }

        public ThreadSafeList(IEnumerable<T> list)
        {
            _list = list.ToList();
        }



        // Other Elements of IList implementation



        public int Count => Lock(() => _list.Count);

        public bool IsReadOnly => Lock(() => _list.ToType<IList>().IsReadOnly);

        public T this[int index]
        {
            get => Lock(() => _list[index]);
            set => Lock(() => _list[index] = value);
        }

        public int IndexOf( T item )
        {
            return Lock(() => _list.IndexOf(item));
        }

        public void Insert( int index, T item )
        {
            Lock(() => _list.Insert(index, item) );
        }

        public void RemoveAt( int index )
        {
            Lock(() => _list.RemoveAt(index)) ;
        }

        public void Add( T item )
        {
            Lock(() => _list.Add(item) ) ;
        }

        public void Clear()
        {
            Lock(() => _list.Clear()) ;
        }

        public bool Contains( T item )
        {
            return Lock(() => _list.Contains(item)) ;
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            lock (_key)
            {
                for (int i = arrayIndex; i < array.Length; i++)
                {
                    array[i] = _list[i];
                }
            }            
        }

        public bool Remove( T item )
        {
            return Lock(() => _list.Remove( item ) ) ;
        }


        private D Lock<D>( Func<D> func )
        {
            lock (_key) { return func(); }
        }

        private void Lock( Action func )
        {
            lock (_key) { func(); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_key)
            {
                return _list.ToList().GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_key)
            {
                return _list.ToList().GetEnumerator();
            }
        }

        public void RemoveRange(IEnumerable<T> toBeDeleted)
        {
            lock(_key)
            {
                toBeDeleted.ForEach(item => _list.Remove(item));
            }
        }

        public void Sort(Func<T, object> keySelector)
        {
            lock(_key)
            {
                var org = this.ToArray();
                var sorted = org.OrderBy(keySelector);
                _list.Clear();
                _list.AddRange(sorted);
            }
        }
    }


    [Serializable]
    public class ListSingleItemException : Exception
    {
        public ListSingleItemException() { }
        public ListSingleItemException( string message ) : base(message) { }
        public ListSingleItemException( string message, Exception inner ) : base(message, inner) { }
        protected ListSingleItemException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context ) : base(info, context) { }
    }
}
