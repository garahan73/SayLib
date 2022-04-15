using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Say32;

namespace System.Windows.Forms
{


    public static class ListBoxUtil
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        const uint WM_VSCROLL = 0x0115;
        const uint SB_THUMBPOSITION = 4;

        public static void ScrollTo(this ListBox listBox, int index)
        {
            uint param = ((uint)(index) << 16) |
                          (SB_THUMBPOSITION & 0xffff);

            SendMessage(listBox.Handle, WM_VSCROLL, param, 0);
        }

        public static void ScrollToLast(this ListBox listBox)
        {
            ScrollTo(listBox, listBox.Items.Count -1);
        }

        public static void ScrollToSelectedItem(this ListBox listBox)
        {
            ScrollTo(listBox, listBox.SelectedIndex);
        }

        public static void UpdateItems(this ListBox listBox, IList items)
        {
            // add all items
            if (listBox.Items.Count == 0)
            {
                listBox.SynchronizedInvoke(
                    () => listBox.Items.AddRange(items.Cast<object>().ToArray())
                );
            }
            else
            {
                // remove from list box
                foreach (var o in listBox.Items)
                {
                    if (!items.Contains(o))
                    {
                        listBox.SynchronizedInvoke(() =>
                            listBox.Items.Remove(o));
                    }
                }

                // add new ones
                foreach (var conn in items)
                {
                    if (!listBox.Items.Contains(conn))
                    {
                        listBox.SynchronizedInvoke(() =>
                            listBox.Items.Add(conn));
                    }
                }
            }

            //listBox.RefreshItemTexts();
            listBox.SynchronizedInvoke(()=>
                listBox.Refresh());
        }

    }
}
