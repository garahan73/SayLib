
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Windows.Forms.Dialogs
{
    public partial class InputForm : Form
    {
        public InputForm()
        {
            InitializeComponent();
        }

        public string? Message { get => _label.Text; set => _label.Text = value; }
        public string UserInput => _textbox.Text;

        private void _btnOK_Click(object sender, EventArgs e)
        {            
            Close();
        }

    }
}
