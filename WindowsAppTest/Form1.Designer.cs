namespace test2
{
    partial class Form1
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
            this.btnExit = new System.Windows.Forms.Button();
            this.btnReadMain = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtxtMain = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtxtFootNote = new System.Windows.Forms.RichTextBox();
            this.rtxtReference = new System.Windows.Forms.RichTextBox();
            this.btnReadRef = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(578, 497);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "&Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnReadMain
            // 
            this.btnReadMain.Location = new System.Drawing.Point(472, 497);
            this.btnReadMain.Name = "btnReadMain";
            this.btnReadMain.Size = new System.Drawing.Size(100, 23);
            this.btnReadMain.TabIndex = 0;
            this.btnReadMain.Text = "&Read Main";
            this.btnReadMain.UseVisualStyleBackColor = true;
            this.btnReadMain.Click += new System.EventHandler(this.btnReadMain_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(656, 479);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtxtMain);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(648, 453);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Main Text";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtxtMain
            // 
            this.rtxtMain.Location = new System.Drawing.Point(12, 15);
            this.rtxtMain.Name = "rtxtMain";
            this.rtxtMain.Size = new System.Drawing.Size(625, 424);
            this.rtxtMain.TabIndex = 2;
            this.rtxtMain.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.rtxtFootNote);
            this.tabPage2.Controls.Add(this.rtxtReference);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(648, 453);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "References";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // rtxtFootNote
            // 
            this.rtxtFootNote.Location = new System.Drawing.Point(331, 6);
            this.rtxtFootNote.Name = "rtxtFootNote";
            this.rtxtFootNote.Size = new System.Drawing.Size(299, 424);
            this.rtxtFootNote.TabIndex = 3;
            this.rtxtFootNote.Text = "";
            // 
            // rtxtReference
            // 
            this.rtxtReference.Location = new System.Drawing.Point(14, 6);
            this.rtxtReference.Name = "rtxtReference";
            this.rtxtReference.Size = new System.Drawing.Size(299, 424);
            this.rtxtReference.TabIndex = 3;
            this.rtxtReference.Text = "";
            // 
            // btnReadRef
            // 
            this.btnReadRef.Location = new System.Drawing.Point(359, 497);
            this.btnReadRef.Name = "btnReadRef";
            this.btnReadRef.Size = new System.Drawing.Size(107, 23);
            this.btnReadRef.TabIndex = 0;
            this.btnReadRef.Text = "R&ead Ref";
            this.btnReadRef.UseVisualStyleBackColor = true;
            this.btnReadRef.Click += new System.EventHandler(this.btnReadRef_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.ForeColor = System.Drawing.Color.LimeGreen;
            this.lblInfo.Location = new System.Drawing.Point(13, 502);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(41, 13);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "status";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 530);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnReadRef);
            this.Controls.Add(this.btnReadMain);
            this.Controls.Add(this.btnExit);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnReadMain;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtxtMain;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox rtxtReference;
        private System.Windows.Forms.Button btnReadRef;
        private System.Windows.Forms.RichTextBox rtxtFootNote;
        private System.Windows.Forms.Label lblInfo;
    }
}

