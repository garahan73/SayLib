using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{

    public class FilterEngine
    {
        public string OrSplitter { get; set; } = ";";
        public bool IgnoreCase { get; set; } = true;

        public Func<object, string>? ItemToText { get; set; }

        public T[] FilterItems<T>(string filterText, IEnumerable<T> items)
        {
            if (string.IsNullOrEmpty(filterText))
                return items.ToArray();

            return items.Where(item => FilterItem(filterText, item)).ToArray();
        }

        public bool FilterItem(string filterText, object? item)
        {
            if (item == null)
                return false;

            if (string.IsNullOrEmpty(filterText))
                return true;

            var itemText = ItemToText?.Invoke(item) ?? item.ToString() ?? "";

            if (IgnoreCase)
            {
                itemText = itemText.ToLower();
                filterText = filterText.ToLower();
            }

            // split filter text
            var filterItems = filterText.Split(OrSplitter).Select(s => s.Trim()).Where(s => s != "").ToArray();

            foreach (var filterItem in filterItems)
            {
                // check if any of the filter is being matched
                if (AppllySingleFilterItem(itemText, filterItem))
                    return true;
            }

            return false;
        }

        private bool AppllySingleFilterItem(string itemText, string filterItemText)
        {
            // split filter text to words
            var filterWords = filterItemText.Split(' ').Select(s => s.Trim().ToLower())
                        .Where(s => s != "" && s.Length > 1).ToArray();

            // check item text cotains all words
            var anyWordMismatch = filterWords.Where(w => !itemText.Contains(w)).Any();
            return !anyWordMismatch;
        }
    }
}