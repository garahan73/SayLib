using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace System.Windows.Forms
{
    public class SayComboBox : ComboBox
    {
        public Func<object, string>? ItemToText { get; set; }
        public Brush ItemTextBrush { get; set; } = Brushes.Black;
        public Func<object, Brush>? ItemTextBrushSelector { get; set; }
        public Font? ItemTextFont { get; set; }

        public double ItemHeightMarginByFont { get; set; } = 1.6;

        public SayComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;

            //if (ItemHeightMarginByFont > 0)
            //    ItemHeight = (int)(FontHeight * ItemHeightMarginByFont);

            ItemHeight = 30;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;

            if (Items == null || e.Index >= Items.Count)
                return;

            var item = Items[e.Index];
            if (item == null)
                return;

            var text = ItemToText?.Invoke(item) ?? item.ToString();

            e.DrawBackground();

            e.Graphics.DrawString(text,
                ItemTextFont ?? e.Font,
                ItemTextBrushSelector?.Invoke(item) ?? ItemTextBrush,
                e.Bounds, StringFormat.GenericDefault);

            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();

            //base.OnDrawItem(e);
        }
    }
}
