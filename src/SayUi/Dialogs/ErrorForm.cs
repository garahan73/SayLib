using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Say32;

namespace System.Windows.Forms.Dialogs
{
    public partial class ErrorForm : Form
    {
        private ErrorForm(Exception error, string? message = null, string? title = null )
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterParent;

            _lblMessage.Text = message ?? "";
            Text = title ?? "CIM Error";
            _tbError.Text = error.ToString();
        }

        public static void ShowDialog( Control? owner, Exception error, string? message = null, string? title = null )
        {
            var form = new ErrorForm(error, message, title);
            if (owner == null)
                form.ShowDialog();
            else
                owner.SynchronizedInvoke(() => form.ShowDialog(owner));
        }

        public static void Show( Control? owner, Exception error, string? message = null, string? title = null )
        {
            var form = new ErrorForm(error, message, title);
            if (owner == null)
                form.Show();
            else
                owner.SynchronizedInvoke(() => form.Show(owner));
        }

        public static void ShowDialog( Exception error, string? message = null, string? title = null )
        {
            var form = new ErrorForm(error, message, title);
            form.ShowDialog();
        }

        public static void Show( Exception error, string? message = null, string? title = null )
        {
            var form = new ErrorForm(error, message, title);
            form.Show();
        }

        private void _btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
