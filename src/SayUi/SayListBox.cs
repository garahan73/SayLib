using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Forms;
using Say32;

namespace System.Windows.Forms
{
    public class SayListBox : ListBox
    {
        public Func<object, string>? ItemToText { get; set; }
        public Brush ItemTextBrush { get; set; } = Brushes.Black;
        public Func<object, Brush>? ItemTextBrushSelector { get; set; }
        public Font? ItemTextFont { get; set; }

        public double ItemHeightMarginByFont { get; set; } = 1.6;

        public SayListBox()
        {
            DrawMode = DrawMode.OwnerDrawVariable;

            //if (ItemHeightMarginByFont > 0)
            //    ItemHeight = (int)(FontHeight * ItemHeightMarginByFont);

            ItemHeight = 30;
        }

        protected override void OnDrawItem( DrawItemEventArgs e )
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

        public async void RefreshItemTexts()
        {
            await this;

            for (int i = 0; i < Items.Count; i++)
            {
                Items[i] = Items[i];
            }
        }
    }
}
