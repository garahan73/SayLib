using System;
using System.Collections.Generic;

namespace Say32
{
    public class ListDictionary<TKey, TItem, TItemList> : Dictionary<TKey, TItemList>
        where TItemList : IList<TItem>
    {
        public Func<TItemList> CreateList { get; }

        public ListDictionary( Func<TItemList> createList)
        {
            CreateList = createList;
        }

        public void AddItem(TKey itemName, TItem value)
        {
            if (!ContainsKey(itemName))
                Add(itemName, CreateList());

            this[itemName].Add(value);
        }

        public void AddItems(TKey itemName, IEnumerable<TItem> values)
        {
            if (!ContainsKey(itemName))
                Add(itemName, CreateList());

            this[itemName].AddRange(values);
        }
    }

    public class ListDictionary<TKey, TItem> : ListDictionary<TKey, TItem, List<TItem>>
    {
        public ListDictionary() : base(()=> new List<TItem>())
        {
        }
    }
}
