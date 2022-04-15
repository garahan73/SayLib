namespace System.Windows.Forms.Dialogs
{
    partial class ErrorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
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
            this._tbError = new System.Windows.Forms.TextBox();
            this._btnOK = new System.Windows.Forms.Button();
            this._lblMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _tbError
            // 
            this._tbError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._tbError.Location = new System.Drawing.Point(30, 49);
            this._tbError.Multiline = true;
            this._tbError.Name = "_tbError";
            this._tbError.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this._tbError.Size = new System.Drawing.Size(590, 426);
            this._tbError.TabIndex = 0;
            // 
            // _btnOK
            // 
            this._btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnOK.Location = new System.Drawing.Point(453, 498);
            this._btnOK.Name = "_btnOK";
            this._btnOK.Size = new System.Drawing.Size(167, 74);
            this._btnOK.TabIndex = 1;
            this._btnOK.Text = "OK";
            this._btnOK.UseVisualStyleBackColor = true;
            this._btnOK.Click += new System.EventHandler(this._btnOK_Click);
            // 
            // _lblMessage
            // 
            this._lblMessage.AutoSize = true;
            this._lblMessage.Location = new System.Drawing.Point(30, 20);
            this._lblMessage.Name = "_lblMessage";
            this._lblMessage.Size = new System.Drawing.Size(39, 15);
            this._lblMessage.TabIndex = 2;
            this._lblMessage.Text = "label1";
            // 
            // ErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnOK;
            this.ClientSize = new System.Drawing.Size(653, 593);
            this.Controls.Add(this._lblMessage);
            this.Controls.Add(this._btnOK);
            this.Controls.Add(this._tbError);
            this.Name = "ErrorForm";
            this.Text = "CIM Error";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _tbError;
        private System.Windows.Forms.Button _btnOK;
        private System.Windows.Forms.Label _lblMessage;
    }
}