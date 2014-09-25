using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;

namespace BibleDataLayer
{
    public class Bible
    {
        public RichTextBox rtxt;
        public List<Book> Books { get; set; }
        public int currentVerse;
        public bool isPaused;

        public string GetVerse(string book, int chapter, int[] verse)
        {
            return null;
        }

        public void LoadBible()
        {

        }
        public void BibleParser()
        {
            BibleParser(fileName: @"E:\share\joshua.docx");
        }
        public void BibleParser(string fileName = @"E:\share\joshua.docx")
        {
            try
            {
                // Define file path
                var wordApp = new Application();

                // first deal with the book
                Book book = InsertBook("joshua");

                // Create objects for passing
                object oFile = fileName;
                object oNull = System.Reflection.Missing.Value;
                object oReadOnly = true;

                // Open Document

                var Doc = wordApp.Documents.Open(ref oFile, ref oNull,
                        ref oReadOnly, ref oNull, ref oNull, ref oNull, ref oNull,
                        ref oNull, ref oNull, ref oNull, ref oNull, ref oNull,
                        ref oNull, ref oNull, ref oNull, ref oNull);

                // then insert the book's chapters . . . 
                int sequence = 0;
                bool isChapter = false;
                bool isVerse = false;
                int tmpChapterNo = 0;
                int tmpVerseNo = 0;
                int currentVerseNo = 1;
                double currentSize = 0;

                string text = "";
                string font = "";
                double size = 0;

                Chapter chapter = null;
                // Read each paragraph and show         
                foreach (Paragraph oPara in Doc.Paragraphs)
                {
                    foreach (Range character in oPara.Range.Characters)
                    {
                        bool isDigit = char.IsDigit(character.Text, 0);
                        size = character.Font.Size;

                        if (isDigit)
                        {
                            if (size > 25) // this means it's a chapter
                            {
                                isChapter = true;
                                tmpChapterNo = Convert.ToInt32(tmpChapterNo.ToString() + character.Text);
                            }
                            else if (size == 5.5)
                            {
                                isVerse = true;
                                tmpVerseNo = Convert.ToInt32(tmpVerseNo.ToString() + character.Text);
                            }
                            continue;
                        }

                        if (isChapter)
                        {
                            if (text != "")
                            {
                                if (chapter == null)
                                {
                                    chapter = InsertChapter(book, tmpChapterNo);
                                    InsertVerse(chapter, 1, 0, text, font, currentSize);
                                }
                                else
                                {
                                    InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                                    chapter = InsertChapter(book, tmpChapterNo);
                                }
                            }

                            text = character.Text;
                            font = "";
                            isChapter = false;
                            tmpChapterNo = 0;
                            sequence = 0;
                            currentVerseNo = 1;
                            continue;
                        }

                        if (isVerse && text != "")
                        {
                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            sequence = 0;
                            currentVerseNo++;

                            text = character.Text;
                            font = character.Font.Name;
                            isVerse = false;
                            tmpVerseNo = 0;
                            continue;
                        }

                        if (font != character.Font.Name && font != "" && text != "")
                        {
                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            text = "";
                        }


                        if (size == 9.5 || size == 5.5 || size == 9.0)
                        {
                            text += character.Text;
                            font = character.Font.Name;
                            currentSize = size;
                        }
                    }
                }

                // Quit Word
                wordApp.Quit(ref oNull, ref oNull, ref oNull);
            }
            catch (Exception ex)
            {

            }
        }

        public string InsertVerse(Chapter chapter, int verseNo, int sequence, string verseText, string fontName, double size)
        {
            size += 5;
            if (currentVerse != verseNo)
            {
                currentVerse = verseNo;
                rtxt.SelectionFont = new System.Drawing.Font("Times New Roman", (float)5.5, FontStyle.Regular);
                rtxt.SelectionColor = Color.ForestGreen;
                rtxt.AppendText(verseNo.ToString());
            }

            if (sequence > 1)
            {
                rtxt.SelectionColor = Color.Red;
                rtxt.SelectionFont = new System.Drawing.Font("Times New Roman", (float)9, FontStyle.Regular);
                rtxt.AppendText(string.Format("SEQ{0}",sequence));
            }

            switch (fontName)
            {
                case "VG2Main":
                    rtxt.SelectionFont = new System.Drawing.Font("VG2 Main", (float)size, FontStyle.Regular);
                    break;
                case "VG2Title":
                    rtxt.SelectionFont = new System.Drawing.Font("VG2 Title", (float)size, FontStyle.Regular);
                    break;
                case "VG2Agazian":
                    rtxt.SelectionFont = new System.Drawing.Font("VG2 Agazian", (float)size, FontStyle.Regular);
                    break;
                case "TimesNewRoman":
                    rtxt.SelectionFont = new System.Drawing.Font("Times New Roman", (float)size, FontStyle.Regular);
                    break;
                default:
                    rtxt.SelectionFont = new System.Drawing.Font(fontName, (float)size, FontStyle.Regular);
                    break;
            }

            rtxt.SelectionColor = Color.Black;
            rtxt.AppendText(verseText);//.Replace("μ", "µ"));//.Replace("oe","œ"));
            rtxt.ScrollToCaret();

            do
            {
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(1000);
            } while (isPaused);

            //return string.Format("CH: {0} ,VR: {1}, SQ: {2}, TXT: {3}, FNT: {4}, SIZ: {5}", chapter.Id, verseNo, sequence, verseText, 121, size);
            return null;
        }

        private Book InsertBook(string name)
        {
            Book book = new Book();
            book.Name = name;

            book.Id = 6;
            //rtxt.SelectionFont = new System.Drawing.Font("Times New Roman", 11, FontStyle.Regular);
            //rtxt.SelectionColor = Color.YellowGreen;
            //rtxt.AppendText(Environment.NewLine + string.Format("BOOK => {0}", book.Name));
            //rtxt.ScrollToCaret();
            Thread.Sleep(1000);
            return book;
        }

        private Chapter InsertChapter(Book book, int chapterNo)
        {
            Chapter chapter = new Chapter();
            chapter.Book = book;
            chapter.ChapterNo = chapterNo;

            chapter.Id = chapterNo;
            rtxt.SelectionFont = new System.Drawing.Font("Times New Roman", 11, FontStyle.Regular);
            rtxt.SelectionColor = Color.DarkRed;
            //rtxt.AppendText(Environment.NewLine + string.Format("BOOK => {0}   CHAPTER => {1}", chapter.Book.Name, chapter.ChapterNo));
            rtxt.AppendText(chapter.ChapterNo.ToString());
            rtxt.ScrollToCaret();
            Thread.Sleep(1000);
            return chapter;
        }
    }
}