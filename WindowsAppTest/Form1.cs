﻿using System;
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

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            Bible bible = new Bible();
            //bible.PopulateTestData();
            //return;
            bible.CreateBible();
            bible.BibleParser(fileName: @"E:\share\joshua.docx", bookName: "joshua");


            //List<Verse> verseDetails = bible.GetVerse("joshua", 1, 1);
            //WriteVerse(verseDetails);

            //List<Verse> verseDetails = bible.GetChapter("joshua", 1);
            //WriteVerse(verseDetails);

            List<Chapter> chapters = bible.GetChapters("joshua");

            foreach (var chapter in chapters)
            {
                richTextBox1.SelectionColor = Color.Blue;
                richTextBox1.SelectionFont = new System.Drawing.Font("VG2 Main", (float)22, FontStyle.Bold);
                richTextBox1.AppendText(chapter.ChapterNo.ToString());
                
                WriteVerse(chapter.Verses);
            }

        }

        private void WriteVerse(List<Verse> verseDetails)
        {
            int currentVerseNo = 1;
            foreach (var verse in verseDetails)
            {
                if (currentVerseNo != verse.No  && verse.Sequene != 0)
                {
                    currentVerseNo = verse.No;
                    richTextBox1.SelectionColor = Color.Green;
                    richTextBox1.SelectionFont = new System.Drawing.Font("VG2 Main", (float)8, FontStyle.Bold);
                    richTextBox1.AppendText(currentVerseNo.ToString());
                }

                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.SelectionFont = new System.Drawing.Font(verse.Font.Name, (float)verse.Size, FontStyle.Regular);
                richTextBox1.AppendText(verse.Text);
                richTextBox1.ScrollToCaret();
                //Thread.Sleep(1000);
                Application.DoEvents();
            }
        }
    }
}