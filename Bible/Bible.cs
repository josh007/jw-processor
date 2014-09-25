using System;
using System.Collections.Generic;
using System.Globalization;
using PDFReader;
using SQLite;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;

namespace BibleDataLayer
{
    public class Bible
    {
        public SQLiteManager SqlMgr { get; set; }

        public List<Book> Books { get; set; }

        public Bible()
        {
            SqlMgr = new SQLiteManager("Data Source=joshdb.sqlite;Version=3;foreign keys=true;");
        }

        public void LoadBible()
        {

        }

        public void CreateBible()
        {
            //create the repository
            //SqlMgr.CreateDataBase();

            //create tables
            SqlMgr.CreateTableStructures();
        }

        public void PopulateTestData()
        {
            //populate the data
            SqlMgr.PopulateTestData();
        }

        public void BibleParser(string fileName = @"E:\share\joshua.docx", string bookName = "joshua")
        {
            TruncateBibleInfoFromDB(bookName);
            //ImportPDFCleanDataForBook(fileName, bookName);

            try
            {
                // Define file path
                var wordApp = new Application();

                // first deal with the book
                Book book = InsertBook(bookName);

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
                bool isChapter = false;
                bool isVerse = false;
                bool isOkToReadPDFRecord = true;

                int sequence = 0;
                int tmpChapterNo = 0;
                int tmpVerseNo = 0;
                int currentVerseNo = 1;
                
                string text = "";
                string font = "";
                string resultFromPDF = "";
                string resultFromPDFPrev = "";
                string resultFromPDFUncommited = "";

                double size = 0;
                double currentSize = 0;

                Chapter chapter = null;
                int prevRecord = ReadFirstPDFRecord(bookName);
                // Read each paragraph and show         
                foreach (Paragraph oPara in Doc.Paragraphs)
                {
                    // if there is a chapter change, make sure u read only once for the paragraph as the PDF
                    // always has two lines made up in one so i need to compensate for that
                    if (isOkToReadPDFRecord)
                    {
                        resultFromPDFUncommited += (resultFromPDFPrev != "" ? resultFromPDFPrev.Remove(0, 1) : resultFromPDF) + "\r\n";
                        resultFromPDFPrev = resultFromPDF;
                        resultFromPDF = ReadPDFRecord(prevRecord, out prevRecord);
                    }
                    else // reset it for the nxt paragraph read
                        isOkToReadPDFRecord = true;

                    foreach (Range character in oPara.Range.Characters)
                    {
                        bool isDigit = char.IsDigit(character.Text, 0);
                        size = character.Font.Size;

                        if (isDigit)
                        {
                            if (size > 25) // this means it's a chapterNo
                            {
                                isChapter = true;
                                tmpChapterNo = Convert.ToInt32(tmpChapterNo.ToString() + character.Text);
                            }
                            else if (size == 5.5) // means it's a verseNo
                            {
                                isVerse = true;
                                tmpVerseNo = Convert.ToInt32(tmpVerseNo.ToString() + character.Text);
                            }
                            else if (size == 7) // means a reference coming b4 z foot-note
                            {
                                
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
                                    //InsertVerse(chapter, 1, 0, text, font, currentSize);
                                    // this is the first page . . . if it has any text it's already processed
                                    InsertVerse(chapter, 1, 0, resultFromPDF.Remove(0, 1), font, currentSize);
                                }
                                else
                                {
                                    if (Convert.ToInt32(resultFromPDFPrev[0]) != 0) // means its' a heading or something
                                    {
                                        int len = resultFromPDFUncommited.Length - resultFromPDFPrev.Length - 1;
                                        text = resultFromPDFUncommited.Remove(len, resultFromPDFPrev.Length - 1);
                                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                                        chapter = InsertChapter(book, tmpChapterNo);
                                        InsertVerse(chapter, 1, 0, resultFromPDFPrev.Remove(0, 1), font, currentSize);
                                    }
                                    else
                                    {
                                        text = resultFromPDFUncommited.Remove(0, 1);
                                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                                        chapter = InsertChapter(book, tmpChapterNo);
                                    }
                                }
                            }

                            // means there is some other characters on top of the chapter
                            // which we don't want to process anyways; just process the chapter and continue;
                            // illegal???????????
                            if(text == "" && tmpChapterNo > 0) 
                                chapter = InsertChapter(book, tmpChapterNo);

                            text = character.Text;
                            font = "";
                            isChapter = false;
                            isOkToReadPDFRecord = false;
                            resultFromPDFPrev = "";
                            resultFromPDFUncommited = "";
                            tmpChapterNo = 0;
                            sequence = 0;
                            currentVerseNo = 1;
                            continue;
                        }

                        if (isVerse && text != "")
                        {
                            int len = resultFromPDFUncommited.Length - 1;
                            text = resultFromPDFUncommited.Remove(text.Length - 1, len - text.Length);
                            
                            resultFromPDFUncommited =  (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length - 1 + tmpVerseNo.ToString().Length) : "");
                            resultFromPDFPrev = "";

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
                            int len = resultFromPDFUncommited.Length - 1;
                            text = resultFromPDFUncommited.Remove(text.Length - 1, len - text.Length);

                            resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length - 1 + tmpVerseNo.ToString().Length) : "");
                            resultFromPDFPrev = "";

                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            text = "";
                        }

                        // this is the main text and verse #s
                        if (size == 9.5 || size == 5.5 || size == 9.0)
                        {
                            text += character.Text;
                            font = character.Font.Name;
                            currentSize = size;
                        }
                        else if (size < 5) // means it's a foot-note;
                        {
                            
                        }
                        else if (size == 6 || size == 7) // means it's a reference
                        {
                            
                        }
                    }
                }

                InsertVerse(chapter, currentVerseNo, ++sequence, resultFromPDFUncommited, font, currentSize);
                // Quit Word
                wordApp.Quit(ref oNull, ref oNull, ref oNull);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string ReadPDFRecord(int prevRecord, out int nextRecord)
        {
            return SqlMgr.ReadPDFRecord(prevRecord, out nextRecord);
        }

        private int ReadFirstPDFRecord(string bookName)
        {
            return SqlMgr.ReadFirstPDFRecord(bookName) - 1;// so we can include the first record as well
        }

        private void ImportPDFCleanDataForBook(string fileName, string bookName)
        {
            Console.WriteLine("importing pdf data . .. ");
            ImportDataToSQLite importer = new ImportDataToSQLite();
            importer.Import(fileName, bookName);
        }

        private void TruncateBibleInfoFromDB(string bookName)
        {
            SqlMgr.TruncateBibleInfoFromDb(bookName);
            Console.WriteLine("deleting . .. ");
        }

        public void InsertVerse(Chapter chapter, int verseNo, int sequence, string verseText, string fontName, double size)
        {
            switch (fontName)
            {
                case "VG2Main":
                    fontName = "VG2 Main";
                    break;
                case "VG2Title":
                    fontName = "VG2 Title";
                    break;
                case "VG2Agazian":
                    fontName = "VG2 Agazian";
                    break;
                case "TimesNewRoman":
                    fontName = "Times New Roman";
                    break;
                default:
                    break;
            }

            Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2} |text-{3} |F{4}({5})",
                chapter.ChapterNo, verseNo, sequence, verseText, fontName, size);

            SqlMgr.InsertVerse(chapter.Id, verseNo, sequence, verseText, fontName, size);
        }

        private Chapter InsertChapter(Book book, int chapterNo)
        {
            Console.WriteLine("inserting chapter: {0}", chapterNo);

            Chapter chapter = new Chapter();
            chapter.Book = book;
            chapter.ChapterNo = chapterNo;

            chapter.Id = SqlMgr.InsertChapter(chapter.Book.Id, chapter.ChapterNo);

            return chapter;
        }

        private Book InsertBook(string bookName)
        {
            Console.WriteLine("inserting book: {0}", bookName);
            Book book = new Book();
            book.Name = bookName;

            book.Id = SqlMgr.InsertBook(book.Name);

            return book;
        }

        public List<Chapter> GetChapters(string bookName)
        {
            List<Chapter> bookChapters = new List<Chapter>();

            int NoOfChapters = SqlMgr.GetNoOfChapters(bookName);
            Dictionary<int, List<SQLiteManager.VerseResult>> chapters = SqlMgr.GetChapters(bookName);

            foreach (var chapter in chapters)
            {
                var chap = new Chapter();
                chap.ChapterNo = chapter.Key;
                //cc.Verses = 
                foreach (var verse in chapter.Value)
                {
                    chap.Verses.Add(new Verse
                        {
                            Chapter = new Chapter { ChapterNo = chap.ChapterNo, Book = new Book { Name = bookName } },
                            No = verse.VerseNo,
                            Sequene = verse.Sequence,
                            Text = verse.Text,
                            Font = new Font{ Name = verse.FontName},
                            Size = verse.Size
                        });
                }
                bookChapters.Add(chap);
            }

            return bookChapters;
        }

        public List<Verse> GetChapter(string bookName, int chapterNo)
        {
            List<Verse> chapterVerses = new List<Verse>();

            int NoOfVerses = SqlMgr.GetNoOfVerses(bookName, chapterNo);
            List<SQLiteManager.VerseResult> verses = SqlMgr.GetChapter(bookName, chapterNo);

            foreach (var verse in verses)
            {
                chapterVerses.Add(new Verse
                    {
                        Chapter = new Chapter { ChapterNo = chapterNo, Book = new Book { Name = bookName } },
                        No = verse.VerseNo,
                        Sequene = verse.Sequence,
                        Text = verse.Text,
                        Font = new Font { Name = verse.FontName },
                        Size = verse.Size
                    });
            }

            return chapterVerses;
        }

        public List<Verse> GetVerse(string bookName, int chapterNo, int verseNo)
        {
            return GetVerse(bookName, chapterNo, verseNo, verseNo);
        }

        public List<Verse> GetVerse(string bookName, int chapterNo, int verseNoStart, int verseNoEnd)
        {
            List<Verse> result = new List<Verse>();

            List<SQLiteManager.VerseResult> verseDetails = SqlMgr.GetVerse(bookName, chapterNo, verseNoStart, verseNoEnd);

            foreach (var verse in verseDetails)
            {
                result.Add(new Verse
                    {
                        Chapter = new Chapter { ChapterNo = chapterNo, Book = new Book { Name = bookName } },
                        No = verse.VerseNo,
                        Sequene = verse.Sequence,
                        Text = verse.Text,
                        Font = new Font { Name = verse.FontName },
                        Size = verse.Size
                    });
            }

            return result;
        }
    }
}