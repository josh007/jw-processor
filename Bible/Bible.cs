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
        public enum RefType
        {
            NONE = 0,
            HEADING = 1,
            FOOTNOTE = 2,
            REF = 3
        }

        public SQLiteManager SqlMgr { get; set; }

        public List<Book> Books { get; set; }

        private string tmpUncommitedRef;
        private int tmpSequence = 0;
        private int tmpVerseId = 0;
        private bool isFootNote;
        private string tmpUncommitedRefFontChangeCharacter;

        public Bible()
        {
            SqlMgr = new SQLiteManager("Data Source=joshdb1.sqlite;Version=3;foreign keys=true;");
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
                int skipedLines = 0;

                string text = "";
                string font = "";
                string resultFromPDF = "";
                string resultFromPDFPrev = "";
                string resultFromPDFUncommited = "";

                double size = 0;
                double currentSize = 0;

                Chapter chapter = null;
                Dictionary<int, string> footNotes = new Dictionary<int, string>();

                int prevRecord = ReadFirstPDFRecord(bookName);
                int tmpRecord;
                // Read each paragraph and show         
                foreach (Paragraph oPara in Doc.Paragraphs)
                {

                    // if there is a chapter change, make sure u read only once for the paragraph as the PDF
                    // always has two lines made up in one so i need to compensate for that
                    if (isOkToReadPDFRecord)
                    {
                        if (resultFromPDFUncommited != "" && resultFromPDFUncommited.Length - 1 != text.Length)
                            text = resultFromPDFUncommited.Remove(resultFromPDFUncommited.Length - 1, 1);

                        if (resultFromPDFUncommited != "")
                        {
                            tmpRecord = prevRecord;
                            resultFromPDFUncommited =
                                                      resultFromPDFUncommited.Remove(resultFromPDFUncommited.Length - 1, 1) +
                                                        ReadPDFRecord(prevRecord, out prevRecord).Remove(0, 1) + "\r\n";
                            prevRecord = tmpRecord;
                        }
                        else if (resultFromPDFUncommited == "" && resultFromPDF != "")
                            resultFromPDFUncommited += resultFromPDF + "\r\n";
                        else if (resultFromPDFPrev != "")
                            resultFromPDFUncommited += resultFromPDFPrev.Remove(0, 1);


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
                            #region 

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

                            #endregion

                        }

                        if (isChapter)
                        {
                            #region 

                            if (text != "")
                            {
                                if (chapter == null)
                                {
                                    chapter = InsertChapter(book, tmpChapterNo);
                                    //InsertVerse(chapter, 1, 0, text, font, currentSize);
                                    // this is the first page . . . if it has any text it's already processed
                                    InsertVerse(chapter, 1, 0, resultFromPDFUncommited.Remove(0, 1), font, currentSize);
                                }
                                else
                                {
                                    // means everything is ok so u can simply replace it
                                    if (
                                        resultFromPDFUncommited.Remove(text.Length,
                                                                       resultFromPDFUncommited.Length - text.Length)
                                                               .Length == text.Length)
                                        text = resultFromPDFUncommited.Remove(text.Length,
                                                                              resultFromPDFUncommited.Length - 1 -
                                                                              text.Length - 1);
                                    else
                                        // means something is wrong; 1. additional character in text or additional character in resultFromPDFUncommited
                                    {
                                        // find the number as this is verse change; forward looking
                                        int index = 0;
                                        for (int i = text.Length - 1; i < resultFromPDFUncommited.Length; i++)
                                        {
                                            if (char.IsDigit(resultFromPDFUncommited, i))
                                            {
                                                index = i;
                                                break;
                                            }
                                        }
                                        // find the number as this is verse change; backward looking
                                        if (index == 0)
                                        {
                                            for (int i = text.Length - 1; i > 0; i--)
                                            {
                                                if (char.IsDigit(resultFromPDFUncommited, i))
                                                {
                                                    index = i;
                                                    break;
                                                }
                                            }
                                        }

                                        if (index == 0)
                                            throw new Exception("this shouldn't happen something is really wrong");

                                        text = resultFromPDFUncommited.Remove(index,
                                                                              resultFromPDFUncommited.Length - 1 -
                                                                              text.Length - 1);
                                    }

                                    // means this is a heading otherwise it's not a heading
                                    if (Convert.ToInt32(resultFromPDFPrev[0]) != 0) // means its' a heading or something
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
                            }

                            // means there is some other characters on top of the chapter
                            // which we don't want to process anyways; just process the chapter and continue;
                            // illegal???????????
                            if (text == "" && tmpChapterNo > 0)
                                chapter = InsertChapter(book, tmpChapterNo);

                            text = character.Text;
                            font = "";
                            isChapter = false;
                            isOkToReadPDFRecord = false;
                            resultFromPDFPrev = "";
                            resultFromPDFUncommited = resultFromPDF.Remove(0, 2).Remove(28, 1).Insert(28, "\r") + "\r\n";
                            tmpChapterNo = 0;
                            sequence = 0;
                            currentVerseNo = 1;
                            continue;

                            #endregion
                        }

                        if (isVerse && text != "")
                        {
                            //TODO: Inbetween title should be done here
                            //int len = text.Length;
                            #region 

                            // means everything is ok so u can simply replace it
                            if (
                                resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length)
                                                       .Length == text.Length)
                                text = resultFromPDFUncommited.Remove(text.Length,
                                                                      resultFromPDFUncommited.Length - 1 - text.Length -
                                                                      1);
                            else
                                // means something is wrong; 1. additional character in text or additional character in resultFromPDFUncommited
                            {
                                // find the number as this is verse change; forward looking
                                int index = 0;
                                for (int i = text.Length - 1; i < resultFromPDFUncommited.Length; i++)
                                {
                                    if (char.IsDigit(resultFromPDFUncommited, i))
                                    {
                                        index = i;
                                        break;
                                    }
                                }
                                // find the number as this is verse change; backward looking
                                if (index == 0)
                                {
                                    for (int i = text.Length - 1; i > 0; i--)
                                    {
                                        if (char.IsDigit(resultFromPDFUncommited, i))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }
                                }

                                if (index == 0)
                                    throw new Exception("this shouldn't happen something is really wrong");

                                text = resultFromPDFUncommited.Remove(index,
                                                                      resultFromPDFUncommited.Length - 1 - text.Length -
                                                                      1);
                            }

                            resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length
                                                           ? resultFromPDFUncommited.Remove(0,
                                                                                            text.Length - 2 +
                                                                                            tmpVerseNo.ToString().Length)
                                                           : "");
                            resultFromPDFPrev = "";

                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            sequence = 0;
                            currentVerseNo++;

                            text = character.Text;
                            font = character.Font.Name;
                            isVerse = false;
                            tmpVerseNo = 0;
                            continue;

                            #endregion
                        }
                        
                        if (font != character.Font.Name && font != "" && text != "")
                        {
                            if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            else // means there is a problem??????? TODO: what will happen . . . .
                            {
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            }

                            resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length) : "");
                            resultFromPDFPrev = "";

                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            text = "";
                        }

                        if (size == 9.5 || size == 9.0) // this is the main text and verse #s
                        {
                            text += character.Text;
                            font = character.Font.Name;
                            currentSize = size;
                        }
                        else if (size == 5.5) // means it's a foot-note marker; it can't b a verse cause verse's has been handled already @ z top
                        {
                            if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            else // means there is a problem??????? TODO: what will happen . . . .
                            {
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            }

                            resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length) : "");
                            resultFromPDFPrev = "";

                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize); // verse b4 z foot-note character
                            int verse_id = InsertVerse(chapter, currentVerseNo, ++sequence, character.Text, character.Font.Name, size); // the foot-note character
                            footNotes.Add(verse_id, character.Text);
                            text = "";
                        }
                        else if (size == 3.5 || size == 6 || size == 7 || size == 11 || size == 14) // means it's a reference foot-note or other
                        {
                            if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            else // means there is a problem??????? TODO: what will happen . . . .
                            {
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            }

                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize); // verse b4 z foot-note character
                            resultFromPDFUncommited = "";
                            resultFromPDFPrev = "";
                            text = "";

                            skipedLines = ProcessReference(oPara, resultFromPDF, chapter, footNotes, size);
                            break;
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

        private int ProcessReference(Paragraph text, string resultFromPDF, Chapter chapter, Dictionary<int, string> footNotes, double size)
        {
            int linesToSkip = 0;

            if (size == 3.5) // footer detail starts this time 
            {
                if (isFootNote) // commit data as the text had already started a foot-note and needs a commit
                    ProcessReferenceText(text, resultFromPDF, footNotes, false);
                else if (tmpUncommitedRef != "") // means there is some text that's a reference
                {
                    // TODO: Commit the text

                    tmpUncommitedRef = "";
                }

                isFootNote = true;
                ProcessReferenceText(text, resultFromPDF, footNotes, false);

                return 0;
            }

            if (size == 6 || size == 7) // no need to further processing as this is just a reference detail
            {
                bool isRefVerse;
                if (text.Range.Text.Contains(" ")) // means a ref wiz verse #
                {
                    isRefVerse = true;
                }
                else // means a verse detail ref for the prev line
                {
                    isRefVerse = false;
                }

                //var splittedRef = text.Range.Text.Split(new[] { "¿" }, StringSplitOptions.None);􀀍 or ,
                var splittedRef = text.Range.Text.Split(new[] { "􀀍" }, StringSplitOptions.None);
                linesToSkip = splittedRef.Length;

                if (isFootNote)
                {
                    if (linesToSkip < 1) // this line is also part of the foot-note
                    {
                        ProcessReferenceText(text, resultFromPDF, footNotes, false);
                        return 0;
                    }

                    ProcessReferenceText(text, resultFromPDF, footNotes, true);
                    isFootNote = false;
                }

                // further processing of,
                splittedRef = text.Range.Text.Split(new[] { "¿" }, StringSplitOptions.None);
                //tmpUncommitedRef = aa.ToString();

                return linesToSkip;
            }

            //page title hence just skip this line and continue . . . 
            if (size == 10 || size == 11 || size == 14) // pageer/page # hence references start . . . 
            {
                if (isFootNote)
                {
                    ProcessReferenceText(text, resultFromPDF, footNotes, true);
                    isFootNote = false;
                }

                if (tmpUncommitedRef != "")
                {
                    //TODO: commit the text

                    tmpUncommitedRef = "";
                }
                return 0;
            }

            return -1;//illegal shouldn't happen
        }

        private void ProcessReferenceText(Paragraph text, string resultFromPDF, Dictionary<int, string> footNotes, bool isFinal)
        {
            string font = "";
            string tmpRef = "";
            string refString = tmpUncommitedRefFontChangeCharacter != "" ? tmpUncommitedRefFontChangeCharacter : tmpUncommitedRef[0].ToString(); ;

            if (isFinal && tmpUncommitedRef != "")
            {
                tmpVerseId = GetVerseRef(footNotes, refString);

                if (tmpVerseId == 0)
                    throw new Exception();

                InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                tmpUncommitedRef = "";
                tmpUncommitedRefFontChangeCharacter = "";
                tmpSequence = 0;
                return;
            }

            foreach (Range character in text.Range.Characters)
            {
                if (font != character.Font.Name && font != "")
                {
                    if (resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length).Length == tmpRef.Length)
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);
                    else // means there is a problem??????? TODO: what will happen . . . .                    
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);
                    tmpUncommitedRef += tmpRef;
                    tmpVerseId = GetVerseRef(footNotes, refString);

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRefFontChangeCharacter = tmpUncommitedRef[0].ToString();
                    tmpUncommitedRef = "";
                    tmpRef = "";
                }

                if (character.Font.Size == 3.5 && tmpUncommitedRef != "") // means there is another foot-note here
                {
                    if (resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length).Length == tmpRef.Length)
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);
                    else // means there is a problem??????? TODO: what will happen . . . .                    
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);

                    tmpUncommitedRef += tmpRef;
                    tmpVerseId = GetVerseRef(footNotes, refString);

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRef = "";
                    tmpUncommitedRefFontChangeCharacter = "";
                    tmpSequence = 0;
                    tmpRef = "";
                }

                tmpRef += character.Text;
                font = character.Font.Name;
            }

            if (resultFromPDF.Remove(tmpUncommitedRef.Length, resultFromPDF.Length - tmpUncommitedRef.Length).Length == tmpUncommitedRef.Length)
                tmpRef = resultFromPDF.Remove(tmpUncommitedRef.Length, resultFromPDF.Length - tmpUncommitedRef.Length);
            else // means there is a problem??????? TODO: what will happen . . . .
                tmpRef = resultFromPDF.Remove(tmpUncommitedRef.Length, resultFromPDF.Length - tmpUncommitedRef.Length);

            tmpUncommitedRef += tmpRef;
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

        public void InsertReference(int chapterId, int verseId, int sequence, string refText, string fontName, int type)
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

            //Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2} |text-{3} |F{4}({5})",
            //    chapter.ChapterNo, verseNo, sequence, verseText, fontName, size);

            SqlMgr.InsertReference(chapterId, verseId, sequence, refText, fontName, type);
        }

        public int InsertVerse(Chapter chapter, int verseNo, int sequence, string verseText, string fontName, double size)
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

            return SqlMgr.InsertVerse(chapter.Id, verseNo, sequence, verseText, fontName, size);
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

        public int GetChapterId(int verseId)
        {
            return SqlMgr.GetChapterId(verseId);
        }

        public List<Chapter> GetChapters(string bookName)
        {
            List<Chapter> bookChapters = new List<Chapter>();

            //int NoOfChapters = SqlMgr.GetNoOfChapters(bookName);
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
                            Font = new Font { Name = verse.FontName },
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

        private int GetVerseRef(Dictionary<int, string> footNotes, string txt)
        {
            foreach (var footNote in footNotes)
            {
                if (footNote.Value == txt)
                    return footNote.Key;
            }
            return 0;
        }
    }
}