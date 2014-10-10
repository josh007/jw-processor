namespace test2
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
            this.btnExit = new System.Windows.Forms.Button();
            this.btnReadMain = new System.Windows.Forms.Button();
            this.tab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtxtMain = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtxtFootNote = new System.Windows.Forms.RichTextBox();
            this.rtxtReference = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.rtxtPDF = new System.Windows.Forms.RichTextBox();
            this.btnReadRef = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnPDF = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.tab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
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
            // tab
            // 
            this.tab.Controls.Add(this.tabPage1);
            this.tab.Controls.Add(this.tabPage2);
            this.tab.Controls.Add(this.tabPage3);
            this.tab.Location = new System.Drawing.Point(12, 12);
            this.tab.Name = "tab";
            this.tab.SelectedIndex = 0;
            this.tab.Size = new System.Drawing.Size(656, 479);
            this.tab.TabIndex = 2;
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
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.rtxtPDF);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(648, 453);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "PDF Fixed";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // rtxtPDF
            // 
            this.rtxtPDF.Location = new System.Drawing.Point(12, 14);
            this.rtxtPDF.Name = "rtxtPDF";
            this.rtxtPDF.Size = new System.Drawing.Size(625, 424);
            this.rtxtPDF.TabIndex = 3;
            this.rtxtPDF.Text = "";
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
            // btnPDF
            // 
            this.btnPDF.Location = new System.Drawing.Point(246, 497);
            this.btnPDF.Name = "btnPDF";
            this.btnPDF.Size = new System.Drawing.Size(107, 23);
            this.btnPDF.TabIndex = 0;
            this.btnPDF.Text = "Read &PDF";
            this.btnPDF.UseVisualStyleBackColor = true;
            this.btnPDF.Click += new System.EventHandler(this.btnPDF_Click);
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(133, 497);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(107, 23);
            this.btnValidate.TabIndex = 0;
            this.btnValidate.Text = "&Validate";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 530);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.tab);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.btnPDF);
            this.Controls.Add(this.btnReadRef);
            this.Controls.Add(this.btnReadMain);
            this.Controls.Add(this.btnExit);
            this.Name = "frmMain";
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnReadMain;
        private System.Windows.Forms.TabControl tab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtxtMain;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox rtxtReference;
        private System.Windows.Forms.Button btnReadRef;
        private System.Windows.Forms.RichTextBox rtxtFootNote;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnPDF;
        private System.Windows.Forms.RichTextBox rtxtPDF;
        private System.Windows.Forms.Button btnValidate;
    }
}

