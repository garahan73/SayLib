using Say32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public static class WinformUtil
    {
        [DllImport("user32.dll")]
        private static extern int ShowWindow( IntPtr hWnd, uint Msg );

        private const uint SW_RESTORE = 0x09;

        public static void Restore( this Form form )
        {
            if (form.WindowState == FormWindowState.Minimized)
            {
                ShowWindow(form.Handle, SW_RESTORE);
            }
        }

        public static int GetLineNumber(this TextBoxBase tb)
        {
            return tb.GetLineFromCharIndex(tb.SelectionStart);
        }

        public static int GetColumnNumber( this TextBoxBase tb )
        {
            return tb.SelectionStart - tb.GetFirstCharIndexFromLine(tb.GetLineNumber());
        }

        public static Task StartGuiTask(Action action) => TaskUtil.StartStaTask(action);



        //public static void WaitTasksAndCloseWindosAsync(Window window, params Task[] tasks) => WaitTasksAsync(window, ()=>window.Close(), tasks);


    }
}