namespace AmharicBibleProcessor
{
    partial class frmMain
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
            this.rtxtMain = new System.Windows.Forms.RichTextBox();
            this.btnQuit = new System.Windows.Forms.Button();
            this.btnExec = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtxtMain
            // 
            this.rtxtMain.Location = new System.Drawing.Point(12, 12);
            this.rtxtMain.Name = "rtxtMain";
            this.rtxtMain.Size = new System.Drawing.Size(560, 576);
            this.rtxtMain.TabIndex = 0;
            this.rtxtMain.Text = "";
            // 
            // btnQuit
            // 
            this.btnQuit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnQuit.Location = new System.Drawing.Point(498, 594);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(75, 23);
            this.btnQuit.TabIndex = 1;
            this.btnQuit.Text = "&Quit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // btnExec
            // 
            this.btnExec.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExec.Location = new System.Drawing.Point(417, 594);
            this.btnExec.Name = "btnExec";
            this.btnExec.Size = new System.Drawing.Size(75, 23);
            this.btnExec.TabIndex = 2;
            this.btnExec.Text = "&Exec";
            this.btnExec.UseVisualStyleBackColor = true;
            this.btnExec.Click += new System.EventHandler(this.btnExec_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 625);
            this.Controls.Add(this.btnExec);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.rtxtMain);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Processor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtxtMain;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Button btnExec;
    }
}

