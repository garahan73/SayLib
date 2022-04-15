using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Dialogs;
using Say32;

namespace System.Windows.Forms
{
    public class Popup
    {
        public static Form MainForm => SayUI.MainWindow ?? throw new Exception($"No main window. Can't pop up");

        public static void Info( string message, MessageBoxButtons buttons = MessageBoxButtons.OK, string? caption = null )
            => Info(MainForm, message, buttons, caption);

        public static void Info( Control parent, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, string? caption = null )
        {
            ShowMessageBox(parent, message, caption ?? "INFO", buttons, MessageBoxIcon.Information);
        }

        public static void Warning(string message, MessageBoxButtons buttons = MessageBoxButtons.OK, Exception? error = null, string? caption = null)
        {
            Warning(MainForm, message, buttons, error, caption);
        }

        public static void Warning( Control parent, string message, MessageBoxButtons buttons = MessageBoxButtons.OK, Exception? error = null, string? caption = null )
        {
            if (error == null)
                ShowMessageBox(parent, message, caption ?? "WARNING", buttons, MessageBoxIcon.Warning);
            else
                ErrorForm.Show(parent, error, message, caption ?? "WARNING");
        }

        public static void Error( string message, Exception? error = null, MessageBoxButtons buttons = MessageBoxButtons.OK, string? caption = null )
            => Error(MainForm, message, error, buttons, caption);

        public static void Error( Control parent, string message, Exception? error = null, MessageBoxButtons buttons = MessageBoxButtons.OK, string? caption = null)
        {
            if (error == null)
                ShowMessageBox(parent, message, caption ?? "ERROR", buttons, MessageBoxIcon.Error);
            else
                ErrorForm.Show(parent, error, message, caption ?? "ERROR");
        }


        public static void ShowMessageBox( Control? parent, string message, string? caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information )
        {
            if (parent != null)
            {
                parent.SynchronizedInvoke(()=>  MessageBox.Show(parent, message, caption ?? "", buttons, icon) );
            }
            else
                MessageBox.Show(message, caption ?? "", buttons, icon);
        }

        //public static void Error( Control owner, string? message, Exception? error = null, MessageBoxButtons buttons = MessageBoxButtons.OK, string? caption = null )
        //{
        //    owner.SynchronizedInvoke(()=>
        //    {
        //        if (error == null)
        //            MessageBox.Show(owner, message, caption ?? "ERROR", buttons, MessageBoxIcon.Error);
        //        else
        //            ErrorForm.ShowDialog(owner, error, message, caption ?? "ERROR");
        //    });
        //}

        public static string Input( string? message = null, string? title = null ) => Input(MainForm, message, title);

        public static string Input(Control? parent, string? message=null, string? title=null)
        {
            var form = new InputForm();
            form.Message = message;

            if (title != null)
                form.Text = title;

            if (parent != null)
                parent.SynchronizedInvoke(() => form.ShowDialog(parent));
            else
                form.ShowDialog();

            return form.UserInput;
        }

        public static Form Show(Control control, string? title = null)
        {
            var form = CreateForm(control);
            return Show(form, title);
        }

        public static Form Show(Form form, string? title = null)
        {
            if (title != null)
                form.Text = title;

            MainForm.SynchronizedInvoke(() => form.Show());

            return form;
        }

        public static DialogResult ShowDialog(Control control, string title, Control? parent = null)
        {
            var form = CreateForm(control);

            DialogResult r = default;

            parent ??= MainForm;

            parent.SynchronizedInvoke(() =>
            {
                form.Text = title;
                form.TopLevel = false;
                //form.Parent = MainForm;

                if (parent != null)
                    r = form.ShowDialog(parent);
                else
                    r = form.ShowDialog();
            });

            return r;
        }

        private static Form CreateForm(Control control)
        {
            var form = new Form();
            form.Width = (int)(control.Width * 1.1);
            form.Height = (int)(control.Height * 1.1+20);

            control.Dock = DockStyle.Fill;
            form.Controls.Add(control);

            form.StartPosition = FormStartPosition.CenterParent;
            return form;
        }
    }
}
