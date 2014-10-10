using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        private string tmpUncommitedRefFontChangeCharacter = "";
        private string tmpUncommitedRef = "";
        private int tmpSequence = 0;
        private int tmpVerseId = 0;
        private bool isFootNote;
        private bool isRefUncomitted;

        public Bible(string ConnectionString)
        {
            SqlMgr = new SQLiteManager(ConnectionString);
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

        public void BibleParser(string fileName, string bookName)
        {
            TruncateBibleInfoFromDB(bookName);
            //ImportPDFCleanDataForBook(fileName, bookName);

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

            int sequence = 0;
            int tmpChapterNo = 0;
            int tmpVerseNo = 0;
            int currentVerseNo = 1;


            string text = "";
            string font = "";

            double size = 0;
            double currentSize = 0;

            Chapter chapter = null;
            Dictionary<int, string> footNotes = new Dictionary<int, string>();
            LinkedList<string> refDetails = new LinkedList<string>();


            // Read each paragraph and show         
            foreach (Paragraph oPara in Doc.Paragraphs)
            {
                // if there is a chapter change, make sure u read only once for the paragraph as the PDF
                // always has two lines made up in one so i need to compensate for that

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

                        // Process remaining reference details ...
                        if (refDetails.Count > 0)
                        {
                            InsertRefDetails(refDetails, chapter);
                        }


                        if (chapter == null)
                        {
                            chapter = InsertChapter(book, tmpChapterNo);

                            if (text != "")
                            {
                                // this is the first page . . . if it has any text it's already processed
                                InsertVerse(chapter, 1, 0, text, font, currentSize);
                            }
                        }
                        else
                        {
                            if (text != "")
                                InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);

                            chapter = InsertChapter(book, tmpChapterNo);
                        }
                        Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2}", chapter.ChapterNo, 1, sequence);


                        text = character.Text;
                        font = "";
                        isChapter = false;
                        tmpChapterNo = 0;
                        sequence = 0;
                        currentVerseNo = 1;

                        continue;

                        #endregion
                    }

                    if (isVerse)
                    {
                        #region

                        // Process remaining reference details ...
                        if (refDetails.Count > 0)
                        {
                            InsertRefDetails(refDetails, chapter);
                        }

                        if (text == "") // means nothing to commit just update ur verse # and continue
                        {
                            sequence = 0;
                            currentVerseNo++;
                            text = character.Text;
                            font = character.Font.Name;
                            isVerse = false;
                            tmpVerseNo = 0;
                            continue;
                        }

                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);

                        // if any verse has been jumped due to font screw up from the authors side . . . 
                        if (Convert.ToInt32(tmpVerseNo) != currentVerseNo + 1)
                        {
                            for (int i = currentVerseNo + 1; i < tmpVerseNo; i++)
                            {
                                InsertVerse(chapter, i, ++sequence, "", font, currentSize);
                                currentVerseNo++;
                            }
                        }


                        sequence = 0;
                        currentVerseNo++;
                        text = character.Text;
                        font = character.Font.Name;
                        isVerse = false;
                        tmpVerseNo = 0;

                        continue;

                        #endregion
                    }

                    if (font != character.Font.Name && font != "" && text.Trim() != "")
                    {
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
                        #region

                        if (text.Trim() != "")
                        {
                            // commit verse b4 z foot-note character
                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                        }

                        if (character.Text == " ")
                        {
                            text += character.Text;
                            font = character.Font.Name;
                            currentSize = size;

                            continue;
                        }
                        
                        // commit the foot-note character
                        int verse_id = InsertVerse(chapter, currentVerseNo, ++sequence, character.Text, character.Font.Name, size);
                        footNotes.Add(verse_id, character.Text);

                        text = "";

                        #endregion
                    }
                    else if (size == 3.5 || size == 6 || size == 7 || size == 11 || size == 10 || size == 14) // means it's a reference foot-note or other
                    {
                        #region

                        if (currentSize == 9 && (character.Font.Name == "VG2Main" || character.Font.Name == "VG2Title") && (character.Text == "$" || character.Text == " ")) // this is an inbetween character eg: tu
                        {
                            text += character.Text;
                            //font = character.Font.Name;
                            //currentSize = size;

                            continue;
                        }

                        if (text.Trim() != "")
                        {
                            // commit the verse b4 z foot-note character
                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                            text = "";
                        }

                        ProcessReference(oPara, chapter, footNotes, size, refDetails);

                        break;

                        #endregion
                    }
                }
            }

            // Process remaining reference details if exists...
            if (refDetails.Count > 0)
                InsertRefDetails(refDetails, chapter);

            if (text != "")
                InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);

            // Quit Word
            wordApp.Quit(ref oNull, ref oNull, ref oNull);

            ProccessAndFixBibleText(fileName, bookName);

        }

        private void ProccessAndFixBibleText(string fileName, string bookName)
        {
            throw new NotImplementedException();
        }

        private void ProcessReference(Paragraph text, Chapter chapter, Dictionary<int, string> footNotes, double size, LinkedList<string> refDetails)
        {
            //int linesToSkip = 0;
            bool isSeparator = false;

            // check for fake reference headers . . .
            if (text.Range.Text.Contains(" ") && size > 5)
            {
                #region

                for (int i = 0; i < text.Range.Text.Length; i++)
                {
                    if (isSeparator)
                    {
                        if (!char.IsDigit(text.Range.Text[i]))
                        {
                            if (text.Range.Text[i] == 56333)//'�')
                                continue;
                            isSeparator = text.Range.Text[i] == ' ';
                            break;
                        }
                        continue;
                    }
                    if (!char.IsDigit(text.Range.Text[i]))
                    {
                        if (text.Range.Text[i] == 56256)//'�')
                        {
                            isSeparator = true;
                            continue;
                        }
                        break;
                    }
                }

                #endregion
            }

            //check to see if the list has data and if so commit it;
            if (isRefUncomitted && isSeparator)
                InsertRefDetails(refDetails, chapter);

            if (size == 3.5) // footer detail starts this time 
            {
                if (isFootNote) // commit data as the text had already started a foot-note and needs a commit
                {
                    ProcessReferenceText(text, footNotes, false);
                    return;
                }

                isFootNote = true;

                ProcessReferenceText(text, footNotes, false);

                return;
            }

            var splittedRef = text.Range.Text.Split(new[] { "􀀍" }, StringSplitOptions.None);
            var linesToSkip = (splittedRef.Length > 1 && splittedRef[0].Trim() == "" ? splittedRef.Length - 1 : splittedRef.Length);

            if (size == 6 || size == 7) // no need to further processing as this is just a reference detail
            {
                if (isFootNote)
                {
                    // check to see if the foot-note itself has some verses/chapters referenced
                    if (linesToSkip > 1)
                    {
                        for (int i = 0; i < splittedRef[0].Length; i++)
                        {
                            if (char.IsLetter(splittedRef[0][i]) || splittedRef[0][i] == ' ')
                                continue;
                            if (i > 3)
                                linesToSkip = 1;

                            break;
                        }
                    }

                    if (linesToSkip <= 1)
                    {
                        ProcessReferenceText(text, footNotes, false);
                        return;
                    }

                    isFootNote = false;
                    if (tmpUncommitedRef != "")
                        ProcessReferenceText(text, footNotes, true);
                    tmpUncommitedRefFontChangeCharacter = "";
                }

                if (isSeparator) // means a ref with verse #
                {
                    splittedRef = text.Range.Text.Split(new[] { " " }, StringSplitOptions.None);
                    string tmp = "";
                    foreach (var str in splittedRef)
                        if (str != splittedRef[0]) tmp += str;

                    refDetails.AddFirst(splittedRef[0]);
                    refDetails.AddLast(tmp);
                    isRefUncomitted = true;
                }
                else // means a verse detail ref for the prev line
                    refDetails.AddLast(text.Range.Text);

                return;
            }

            //page title hence just skip this line and continue . . . 
            if (size == 10 || size == 11 || size == 14) // pageer/page # hence references start . . . 
            {
                if (isFootNote && tmpUncommitedRef != "")
                    ProcessReferenceText(text, footNotes, true);

                isFootNote = false;

                // Process remaining reference details if exists...
                if (refDetails.Count > 0)
                    InsertRefDetails(refDetails, chapter);

                return;
            }

            throw new Exception();//illegal shouldn't happen
        }

        private void ProcessReferenceText(Paragraph text, Dictionary<int, string> footNotes, bool isFinal)
        {
            string font = "";
            string tmpRef = "";

            if (isFinal && tmpUncommitedRef != "")
            {
                tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);

                if (tmpVerseId == 0)
                    throw new Exception();

                InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                tmpUncommitedRef = "";
                tmpUncommitedRefFontChangeCharacter = "";
                footNotes.Remove(tmpVerseId);
                tmpSequence = 0;
                return;
            }

            foreach (Range character in text.Range.Characters)
            {
                if (font != character.Font.Name && font != "")
                {
                    tmpUncommitedRef += tmpRef;

                    tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRefFontChangeCharacter = (tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);

                    tmpUncommitedRef = "";
                    tmpRef = "";
                }

                if (character.Font.Size == 3.5 && (tmpUncommitedRef != "" || tmpRef != "")) // means there is another foot-note here
                {
                    tmpUncommitedRef += tmpRef;
                    tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter != "" ?
                                                                                tmpUncommitedRefFontChangeCharacter : tmpUncommitedRef[0].ToString());

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRef = "";
                    tmpUncommitedRefFontChangeCharacter = "";

                    footNotes.Remove(tmpVerseId);
                    tmpSequence = 0;
                    tmpRef = "";
                }

                if (tmpRef.Length == 1 && character.Text == "\r" && font == "TimesNewRoman")
                {
                    tmpUncommitedRef += tmpRef;
                    //if(tmpUncommitedRefFontChangeCharacter == "")
                    //    throw  new Exception();

                    tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRefFontChangeCharacter = (tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);
                    tmpUncommitedRef = "";
                    tmpRef = "";
                    break;
                }
                tmpRef += character.Text;
                font = character.Font.Name;
            }

            tmpUncommitedRef += tmpRef;
        }

        //private string ReadPDFRecord(int prevRecord, out int nextRecord, int skipedRecords)
        //{
        //    return SqlMgr.ReadPDFRecord(prevRecord, out nextRecord, skipedRecords);
        //}

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

            Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2} ", chapter.ChapterNo, verseNo, sequence);

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

        private int GetVerseId(int bookId, int chapterNo, int verseNo)
        {
            return SqlMgr.GetVerseId(bookId, chapterNo, verseNo);
        }

        public List<Reference> GetReferences(string bookName)
        {
            var bookReferences = new List<Reference>();

            Dictionary<int, List<ReferenceResult>> references = SqlMgr.GetReferences(bookName);

            foreach (var reference in references)
            {
                foreach (var refer in reference.Value)
                {
                    bookReferences.Add(new Reference
                    {
                        Chapter = new Chapter { ChapterNo = reference.Key, Book = new Book { Name = bookName } },
                        Verse = new Verse { No = refer.VerseNo },
                        Sequene = refer.Sequence,
                        Text = refer.Text,
                        Font = new Font { Name = refer.FontName },
                        RefText = refer.Text[0].ToString(),
                        Type = (RefType)refer.Type
                    });
                }
            }

            return bookReferences;
        }

        public List<Reference> GetReferencesForChapter(string bookName, int chapterNo)
        {
            var chapterReferences = new List<Reference>();

            List<ReferenceResult> references = SqlMgr.GetChapterReferences(bookName, chapterNo);

            foreach (var reference in references)
            {
                chapterReferences.Add(new Reference
                {
                    Chapter = new Chapter { ChapterNo = chapterNo, Book = new Book { Name = bookName } },
                    Verse = new Verse { No = reference.VerseNo },
                    Sequene = reference.Sequence,
                    Text = reference.Text,
                    Font = new Font { Name = reference.FontName },
                    RefText = reference.Text[0].ToString(),
                    Type = (RefType)reference.Type
                });
            }

            return chapterReferences;
        }

        public List<Chapter> GetPDFChapters(string bookName)
        {
            var bookChapters = new List<Chapter>();

            Dictionary<int, List<VerseResult>> chapters = SqlMgr.GetPDFChapters(bookName);

            foreach (var chapter in chapters)
            {
                var chap = new Chapter();
                chap.ChapterNo = chapter.Key;
                
                foreach (var verse in chapter.Value)
                {
                    chap.Verses.Add(new Verse
                    {
                        Chapter = new Chapter { ChapterNo = chap.ChapterNo, Book = new Book { Name = bookName } },
                        No = verse.VerseNo,
                        Text = verse.Text,
                        Type = (RefType)verse.Type
                    });
                }
                bookChapters.Add(chap);
            }

            return bookChapters;
        }

        public List<Chapter> GetChapters(string bookName)
        {
            var bookChapters = new List<Chapter>();

            //int NoOfChapters = SqlMgr.GetNoOfChapters(bookName);
            Dictionary<int, List<VerseResult>> chapters = SqlMgr.GetChapters(bookName);

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
            var chapterVerses = new List<Verse>();

            List<VerseResult> verses = SqlMgr.GetChapter(bookName, chapterNo);

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

            List<VerseResult> verseDetails = SqlMgr.GetVerse(bookName, chapterNo, verseNoStart, verseNoEnd);

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
                    fontName = "VG2 Main";
                    break;
            }

            //Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2} |text-{3} |F{4}({5})",
            //    chapter.ChapterNo, verseNo, sequence, verseText, fontName, size);

            SqlMgr.InsertReference(chapterId, verseId, sequence, refText, fontName, type);
        }

        private void InsertRefDetails(LinkedList<string> refDetails, Chapter chapter)
        {
            string tmpVerseDetail = "";

            var split = refDetails.First.Value.Split(new[] { "􀀍" }, StringSplitOptions.None); // chapter & verse #

            if (split.Length < 2)
            {
                tmpVerseDetail = refDetails.Aggregate(tmpVerseDetail, (current, str) => current + str);

                InsertReferenceException(chapter.Book.Id, chapter.ChapterNo, 0, tmpVerseDetail);

                refDetails.Clear();
                isRefUncomitted = false;

                return;
            }

            if (split[1].Trim() == "")
            {
                string tmp = "";
                int found = -1;

                for (int i = 0; i < refDetails.First.Next.Value.Length; i++)
                {
                    if (char.IsDigit(refDetails.First.Next.Value[i]))
                    {
                        tmp += refDetails.First.Next.Value[i].ToString();
                        found = i;
                        continue;
                    }
                    if (found > 0)
                        break;
                }
                split[1] = tmp;
                refDetails.First.Next.Value = refDetails.First.Next.Value.Remove(0, found + 1);
            }

            int tmpVerseId = GetVerseId(chapter.Book.Id, Convert.ToInt32(split[0]), Convert.ToInt32(split[1]));
            int tmpChapterId = GetChapterId(tmpVerseId);

            tmpVerseDetail = "";
            foreach (var str in refDetails)
                if (str != refDetails.First.Value) tmpVerseDetail += str;

            if (tmpVerseId == 0 || tmpChapterId == 0) // illegal
                InsertReferenceException(chapter.Book.Id, Convert.ToInt32(split[0]), Convert.ToInt32(split[1]), tmpVerseDetail);
            else
                InsertReference(tmpChapterId, tmpVerseId, 0, tmpVerseDetail, null, (int)RefType.REF);

            refDetails.Clear();

            isRefUncomitted = false;
        }

        private void InsertReferenceException(int bookId, int chapterNo, int verseNo, string refText)
        {
            SqlMgr.InsertReferenceException(bookId, chapterNo, verseNo, refText);
        }

        private static string SanitizeUncommited(string resultFromPDFUncommited, string tmpFromPDF)
        {
            resultFromPDFUncommited = tmpFromPDF.Remove(0, 1) + "\r\n";
            do
            {
                if (char.IsDigit(resultFromPDFUncommited[0]))
                    resultFromPDFUncommited = resultFromPDFUncommited.Remove(0, 1);
                else
                    break;
            } while (true);

            return resultFromPDFUncommited;
        }
    }
}