using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public class FilterTextBox : TextBox
    {
        public FilterTextBox()
        {
        }

        public string OrSplitter { get; set; } = ";";
        public bool IgnoreCase { get; set; } = true;

        public Func<object, string>? ItemToText { get; set; }

        public T[] FilterItems<T>( IEnumerable<T> items )
        {
            if (Text == null || Text.Trim() == "")
                return items.ToArray();

            return items.Where(item => Filter(item)).ToArray();
        }

        public bool Filter( object? item )
        {
            if (item == null)
                return false;

            var text = Text;

            if (text == null || text.Trim() == "")
                return true;

            var itemText = ItemToText?.Invoke(item) ?? item.ToString() ?? "";

            if (IgnoreCase)
            {
                itemText = itemText.ToLower();
                text = text.ToLower();
            }

            // split filter text
            var filters = Text.Split(OrSplitter).Select(s => s.Trim()).Where(s => s != "").ToArray();

            foreach (var filterText in filters)
            {
                // check if any of the filter is being matched
                if (Filter(itemText, filterText))
                    return true;
            }

            return false;
        }

        private bool Filter( string itemText, string filterText )
        {
            // split filter text to words
            var filterWords = filterText.Split(' ').Select(s => s.Trim().ToLower())
                        .Where(s => s != "" && s.Length > 1).ToArray();

            // check item text cotains all words
            var anyWordMismatch = filterWords.Where(w => !itemText.Contains(w)).Any();
            return !anyWordMismatch;
        }
    }
}
