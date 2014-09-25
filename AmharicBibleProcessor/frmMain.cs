using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using Font = System.Drawing.Font;


namespace AmharicBibleProcessor
{
    public partial class frmMain : Form
    {
        private BibleDataLayer.Bible bible;
        public frmMain()
        {
            InitializeComponent();
            
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {

                var wordApp = new Application();

                // Define file path
                string fn = @"E:\share\joshua.docx";

                // Create objects for passing
                object oFile = fn;
                object oNull = System.Reflection.Missing.Value;
                object oReadOnly = true;

                // Open Document
                
                var Doc = wordApp.Documents.Open(ref oFile, ref oNull,
                        ref oReadOnly, ref oNull, ref oNull, ref oNull, ref oNull,
                        ref oNull, ref oNull, ref oNull, ref oNull, ref oNull,
                        ref oNull, ref oNull, ref oNull, ref oNull);

                // Read each paragraph and show         
                //foreach (Microsoft.Office.Interop.Word.Paragraph oPara in Doc.Paragraphs)
                //    rtxtMain.AppendText(oPara.Range.Text);

                foreach (Paragraph oPara in Doc.Paragraphs)
                {
                    foreach (Range character in oPara.Range.Characters)
                    {
                        switch (character.Font.Name)
                        {
                            case "VG2Main":
                                rtxtMain.SelectionFont = new Font("VG2 Main", character.Font.Size, FontStyle.Regular);
                                break;
                            case "VG2Title":
                                rtxtMain.SelectionFont = new Font("VG2 Title", character.Font.Size, FontStyle.Regular);
                                break;
                            case "VG2Agazian":
                                rtxtMain.SelectionFont = new Font("VG2 Agazian", character.Font.Size, FontStyle.Regular);
                                break;
                            default:
                                rtxtMain.SelectionFont = new Font("Times New Roman", character.Font.Size, FontStyle.Regular);
                                break;
                        }
                        if (character.Font.Size > 13)
                            break;
                        rtxtMain.AppendText(character.Text);
                        rtxtMain.ScrollToCaret();
                    }
                }

                // Quit Word
                wordApp.Quit(ref oNull, ref oNull, ref oNull);
                this.Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            bible = new BibleDataLayer.Bible();
            bible.rtxt = rtxtMain;

            System.Windows.Forms.Application.DoEvents();
            bible.BibleParser();
        }

        private void btnContiune_Click(object sender, EventArgs e)
        {
            btnContiune.Text = bible.isPaused ? "Pause" : "Continue";
            bible.isPaused = bible.isPaused?false:true;
        }
    }
}
