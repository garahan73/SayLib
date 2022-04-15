
namespace System.Windows.Forms.Dialogs
{
    partial class InputForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._label = new System.Windows.Forms.Label();
            this._textbox = new System.Windows.Forms.TextBox();
            this._btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _label
            // 
            this._label.AutoSize = true;
            this._label.Location = new System.Drawing.Point(12, 9);
            this._label.Name = "_label";
            this._label.Size = new System.Drawing.Size(39, 15);
            this._label.TabIndex = 3;
            this._label.Text = "label1";
            // 
            // _textbox
            // 
            this._textbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._textbox.Location = new System.Drawing.Point(12, 30);
            this._textbox.Name = "_textbox";
            this._textbox.Size = new System.Drawing.Size(277, 23);
            this._textbox.TabIndex = 2;
            // 
            // _btnOK
            // 
            this._btnOK.Location = new System.Drawing.Point(172, 60);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(117, 46);
            this._btnOK.TabIndex = 4;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this._btnOK_Click);
            // 
            // InputForm
            // 
            this.AcceptButton = this._btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(301, 108);
            this.ControlBox = false;
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._label);
            this.Controls.Add(this._textbox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User Input";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _label;
        private System.Windows.Forms.TextBox _textbox;
        private System.Windows.Forms.Button _btnOK;
    }
}