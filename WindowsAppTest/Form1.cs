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
using BibleDataLayer;

namespace test2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //btnReadMain_Click(this, new EventArgs());
            tabControl1.SelectTab(1);
        }
        
        private void WriteVerse(List<Verse> verseDetails)
        {
            int currentVerseNo = 1;
            bool isChapter = true;

            foreach (var verse in verseDetails)
            {
                lblInfo.Text = string.Format("reading text for {0} : {1} ",verse.Chapter.ChapterNo, verse.No);

                // Heading writer anywhere
                if (verse.Sequene == 0)
                {
                    rtxtMain.SelectionColor = Color.Orange;
                    rtxtMain.SelectionFont = new System.Drawing.Font("VG2 Main", (float)11, FontStyle.Bold);
                    rtxtMain.AppendText(Environment.NewLine + verse.Text + Environment.NewLine);
                    continue;
                }

                // Chapter # writer
                if (currentVerseNo == 1 && verse.No == 1 && isChapter)
                {
                    rtxtMain.SelectionColor = Color.DeepSkyBlue;
                    rtxtMain.SelectionFont = new System.Drawing.Font("VG2 Main", (float)22, FontStyle.Bold);
                    rtxtMain.AppendText(verse.Chapter.ChapterNo.ToString());
                    isChapter = false;
                }

                // Verse # writer
                if (currentVerseNo != verse.No && verse.Sequene != 0)
                {
                    currentVerseNo = verse.No;
                    rtxtMain.SelectionColor = Color.Green;
                    rtxtMain.SelectionFont = new System.Drawing.Font("VG2 Main", (float)8, FontStyle.Bold);
                    rtxtMain.AppendText(currentVerseNo.ToString());
                }

                rtxtMain.SelectionColor = Color.Black;
                rtxtMain.SelectionFont = new System.Drawing.Font(verse.Font.Name, (float)verse.Size, FontStyle.Regular);
                rtxtMain.AppendText(verse.Text);
                rtxtMain.ScrollToCaret();
                //Thread.Sleep(1000);
                Application.DoEvents();
            }
        }

        private void WriteReference(IEnumerable<Reference> references)
        {
            int currentVerseNo = 0;

            foreach (var reference in references)
            {
                lblInfo.Text = string.Format("reading ref for {0} : {1} ", reference.Chapter.ChapterNo, reference.Verse.No);

                if (reference.Type == Bible.RefType.FOOTNOTE)
                {
                    if (currentVerseNo != reference.Verse.No)
                    {
                        rtxtFootNote.SelectionColor = Color.Orange;
                        rtxtFootNote.SelectionFont = new System.Drawing.Font("Times New Roman", (float)10, FontStyle.Bold);
                        rtxtFootNote.AppendText(Environment.NewLine + reference.Chapter.ChapterNo + ":" + reference.Verse.No + Environment.NewLine);
                        currentVerseNo = reference.Verse.No;

//                        reference.Text = reference.Text.Remove(0, 1);
                        for (int i = 1; i < reference.Text.Length; i++)
                        {
                              if(char.IsDigit(reference.Text[i]))
                                  continue;
                            reference.Text = reference.Text.Remove(1, i - 1);
                            break;
                        }
                    }

                    rtxtFootNote.SelectionColor = Color.DodgerBlue;
                    rtxtFootNote.SelectionFont = new System.Drawing.Font(reference.Font.Name, (float)9, FontStyle.Regular);
                    rtxtFootNote.AppendText(reference.Text);

                    continue;
                }

                rtxtReference.SelectionColor = Color.Green;
                rtxtReference.SelectionFont = new System.Drawing.Font("Times New Roman", (float)10, FontStyle.Bold);
                rtxtReference.AppendText(reference.Chapter.ChapterNo + ":" + reference.Verse.No + " ");

                rtxtReference.SelectionColor = Color.Black;
                rtxtReference.SelectionFont = new System.Drawing.Font("VG2 Main", (float)9, FontStyle.Regular);
                reference.Text = reference.Text.Replace((char) 56256, ' ');
                reference.Text = reference.Text.Replace((char) 56333, 'Ý');
                rtxtReference.AppendText(reference.Text.Replace(" ", "") + Environment.NewLine + Environment.NewLine);
                
                rtxtReference.ScrollToCaret();

                Application.DoEvents();
            }
        }

        private void btnReadMain_Click(object sender, EventArgs e)
        {
            rtxtMain.Clear();
            tabControl1.SelectTab(0);
            var fileName = @"C:\Users\Administrator\Documents\Visual Studio 2012\Projects\jw-processor\ConsoleAppTest\bin\Debug\joshdb.sqlite";
            Bible bible = new Bible(ConnectionString: "Data Source=" + fileName + ";Version=3;foreign keys=true;");
            //bible.PopulateTestData();
            //return;
            //bible.CreateBible();

            //bible.BibleParser(fileName: @"E:\share\joshua.docx", bookName: "joshua");


            //List<Verse> verseDetails = bible.GetVerse("joshua", 1, 1);
            //WriteVerse(verseDetails);

            //List<Verse> verseDetails = bible.GetChapter("joshua", 1);
            //WriteVerse(verseDetails);

            List<Chapter> chapters = bible.GetChapters("joshua");

            foreach (var chapter in chapters)
            {
                lblInfo.Text = "Processing CHAPTER " + chapter.ChapterNo;
                WriteVerse(chapter.Verses);
            }

        }

        private void btnReadRef_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
            rtxtReference.Clear();
            rtxtFootNote.Clear();

            var fileName = @"C:\Users\Administrator\Documents\Visual Studio 2012\Projects\jw-processor\ConsoleAppTest\bin\Debug\joshdb.sqlite";
            Bible bible = new Bible(ConnectionString: "Data Source=" + fileName + ";Version=3;foreign keys=true;");
            //bible.PopulateTestData();
            //return;
            //bible.CreateBible();

            //bible.BibleParser(fileName: @"E:\share\joshua.docx", bookName: "joshua");


            //List<Verse> verseDetails = bible.GetVerse("joshua", 1, 1);
            //WriteVerse(verseDetails);

            //List<Verse> verseDetails = bible.GetChapter("joshua", 1);
            //WriteVerse(verseDetails);
            var references = bible.GetReferences("joshua");
            WriteReference(references);

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
