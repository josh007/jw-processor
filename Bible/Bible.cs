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
            LinkedList<string> refDetails = new LinkedList<string>();

            int prevRecord = ReadFirstPDFRecord(bookName);
            string tmpFromPDF = "";

            // Read each paragraph and show         
            foreach (Paragraph oPara in Doc.Paragraphs)
            {
                // if there is a chapter change, make sure u read only once for the paragraph as the PDF
                // always has two lines made up in one so i need to compensate for that
                if (isOkToReadPDFRecord)
                {

                    //resultFromPDFUncommited = "";
                    //resultFromPDFPrev = "";

                    tmpFromPDF = ReadPDFRecord(prevRecord, out prevRecord, skipedLines);
                    skipedLines = 0;

                    if (resultFromPDFUncommited != "" && resultFromPDFUncommited.Length - 1 != text.Length)
                        text = resultFromPDFUncommited.Remove(resultFromPDFUncommited.Length - 1, 1);

                    if (resultFromPDFUncommited != "")
                    {
                        resultFromPDFUncommited = resultFromPDFUncommited.Remove(resultFromPDFUncommited.Length - 1, 1) +
                                                    tmpFromPDF.Remove(0, 1) + "\r\n";
                    }
                    else if (resultFromPDFUncommited == "" && resultFromPDF != "")
                    {
                        resultFromPDFUncommited += resultFromPDF + "\r\n";
                    }
                    else if (resultFromPDFPrev != "")
                    {
                        throw new Exception(); // illegal
                        resultFromPDFUncommited += resultFromPDFPrev.Remove(0, 1);
                    }

                    resultFromPDFPrev = resultFromPDF;
                    resultFromPDF = tmpFromPDF;
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

                        // Process remaining reference details ...
                        if (refDetails.Count > 0)
                        {
                            InsertRefDetails(refDetails, chapter);
                        }

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
                                // sanitize the result as it's empty i did it when i process the damn references .....
                                if (resultFromPDFUncommited == "" && text != "")
                                    resultFromPDFUncommited = SanitizeUncommited(resultFromPDFUncommited, tmpFromPDF);


                                // means everything is ok so u can simply replace it
                                if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                                {
                                    text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                                    //text = text.Remove(0, resultFromPDFPrev.Length + 1);
                                    //text = text.Remove(text.Length - resultFromPDFPrev.Length, resultFromPDFPrev.Length);
                                }
                                else
                                // means something is wrong; 1. additional character in text or additional character in resultFromPDFUncommited
                                {
                                    #region

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

                                    #endregion
                                }

                                // means this is a heading otherwise it's not a heading
                                if (Convert.ToInt32(resultFromPDFPrev[0].ToString()) == (int)RefType.HEADING) // means its' a heading
                                {
                                    text = text.Remove(text.Length - resultFromPDFPrev.Length, resultFromPDFPrev.Length);
                                    if (text != "")
                                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);

                                    chapter = InsertChapter(book, tmpChapterNo);
                                    InsertVerse(chapter, 1, 0, resultFromPDFPrev.Remove(0, 1), font, currentSize);
                                }
                                else
                                {
                                    InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                                    chapter = InsertChapter(book, tmpChapterNo);
                                }
                            }
                            Console.WriteLine("inserting verse: C{0} |V{1} |SEQ{2} |text-{3} |F{4}({5})",
                                chapter.ChapterNo, 1, sequence, text, font, size);
                            Console.ReadLine();
                        }

                        // means there is some other characters on top of the chapter
                        // which we don't want to process anyways; just process the chapter and continue;
                        // illegal???????????
                        //if (text == "")
                        //    throw new Exception(); //illegal

                        //if (text == "" && tmpChapterNo > 0)
                        //    chapter = InsertChapter(book, tmpChapterNo);

                        text = character.Text;
                        font = "";
                        isChapter = false;
                        isOkToReadPDFRecord = false;
                        resultFromPDFPrev = "";
                        resultFromPDFUncommited = resultFromPDF.Remove(0, tmpChapterNo.ToString().Length).Remove(28, 1).Insert(28, "\r") + "\r\n";
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
                            resultFromPDFPrev = "";
                            text = character.Text;
                            font = character.Font.Name;
                            isVerse = false;
                            tmpVerseNo = 0;
                            resultFromPDF = resultFromPDF.Remove(1, currentVerseNo.ToString().Length);//hackkked
                            continue;
                        }

                        // sanitize the result as it's empty i did it when i process the damn references .....
                        if (resultFromPDFUncommited == "" && text != "")
                            resultFromPDFUncommited = SanitizeUncommited(resultFromPDFUncommited, tmpFromPDF);


                        // means everything is ok so u can simply replace it
                        if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                            text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - 1 - text.Length - 1);
                        else// means something is wrong; 1. additional character in text or additional character in resultFromPDFUncommited
                        {
                            // find the number as this is verse change; forward looking
                            #region

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

                            #endregion
                        }

                        resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length
                                                       ? resultFromPDFUncommited.Remove(0, text.Length - 2 + tmpVerseNo.ToString().Length) : "");

                        //means it's inbetween title
                        if (resultFromPDFPrev != "" && Convert.ToInt32(resultFromPDFPrev[0].ToString()) == (int)RefType.HEADING)
                        {
                            resultFromPDFUncommited = resultFromPDFUncommited.Remove(resultFromPDFUncommited.Length - resultFromPDFPrev.Length,
                                resultFromPDFPrev.Length - 1);

                            InsertVerse(chapter, currentVerseNo, ++sequence, resultFromPDFUncommited, font, currentSize);
                            InsertVerse(chapter, currentVerseNo + 1, 0, resultFromPDFPrev, font, currentSize);
                            resultFromPDFUncommited = resultFromPDF.Remove(0, tmpVerseNo.ToString().Length);

                            // if any verse has been jumped due to font screw up from the authors side . . . 
                            if (Convert.ToInt32(tmpVerseNo) != currentVerseNo + 1)
                            {
                                for (int i = currentVerseNo + 1; i < tmpVerseNo; i++)
                                {
                                    InsertVerse(chapter, i, ++sequence, "", font, currentSize);
                                    currentVerseNo++;
                                }
                            }
                        }
                        else
                        {
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
                        }

                        sequence = 0;
                        currentVerseNo++;
                        resultFromPDFPrev = "";
                        text = character.Text;
                        font = character.Font.Name;
                        isVerse = false;
                        tmpVerseNo = 0;

                        continue;

                        #endregion
                    }

                    if (font != character.Font.Name && font != "" && text != "")
                    {
                        #region

                        // sanitize the result as it's empty i did it when i process the damn references .....
                        if (resultFromPDFUncommited == "" && text != "")
                            resultFromPDFUncommited = SanitizeUncommited(resultFromPDFUncommited, tmpFromPDF);

                        if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                            text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                        else // means there is a problem??????? TODO: what will happen . . . .
                        {
                            text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                        }

                        if (text == "")
                            throw new Exception(); //illegal

                        resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length) : "");
                        resultFromPDFPrev = "";

                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize);
                        text = "";

                        #endregion
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

                        if (text.Trim() == "")
                        {
                            text = " ";
                            continue;
                        }

                        if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                            text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                        else // means there is a problem??????? TODO: what will happen . . . .
                        {
                            text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                        }

                        if (text == "")
                            throw new Exception(); //illegal

                        resultFromPDFUncommited = (resultFromPDFUncommited.Length > text.Length ? resultFromPDFUncommited.Remove(0, text.Length) : "");
                        resultFromPDFPrev = "";

                        InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize); // verse b4 z foot-note character
                        int verse_id = InsertVerse(chapter, currentVerseNo, ++sequence, character.Text, character.Font.Name, size); // the foot-note character
                        footNotes.Add(verse_id, character.Text);
                        text = "";
                        resultFromPDFUncommited = resultFromPDFUncommited.Remove(0, 1);

                        #endregion
                    }
                    else if (size == 3.5 || size == 6 || size == 7 || size == 11 || size == 14) // means it's a reference foot-note or other
                    {
                        #region

                        if (text != "")
                        {
                            if (resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length).Length == text.Length)
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                            else // means there is a problem??????? TODO: what will happen . . . .
                                text = resultFromPDFUncommited.Remove(text.Length, resultFromPDFUncommited.Length - text.Length);
                        }
                        if (text != "")
                            InsertVerse(chapter, currentVerseNo, ++sequence, text, font, currentSize); // verse b4 z foot-note character

                        resultFromPDFUncommited = "";
                        resultFromPDFPrev = "";
                        text = "";

                        skipedLines = ProcessReference(oPara, resultFromPDF, chapter, footNotes, size, refDetails);
                        resultFromPDF = "";
                        break;

                        #endregion
                    }
                }
            }

            // Process remaining reference details if exists...
            if (refDetails.Count > 0)
            {
                InsertRefDetails(refDetails, chapter);
            }

            if (resultFromPDFUncommited != "")
                InsertVerse(chapter, currentVerseNo, ++sequence, resultFromPDFUncommited, font, currentSize);

            // Quit Word
            wordApp.Quit(ref oNull, ref oNull, ref oNull);

        }

        private int ProcessReference(Paragraph text, string resultFromPDF, Chapter chapter, Dictionary<int, string> footNotes, double size, LinkedList<string> refDetails)
        {
            int linesToSkip = 0;
            bool isSeparator = false;

            // check for fake reference headers . . .
            if (text.Range.Text.Contains(" ") && size > 5)
            {
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

                    isSeparator = false;
                }
            }

            //check to see if the list has data and if so commit it;
            if (isRefUncomitted && isSeparator)
                InsertRefDetails(refDetails, chapter);

            if (size == 3.5) // footer detail starts this time 
            {
                if (isFootNote) // commit data as the text had already started a foot-note and needs a commit
                    ProcessReferenceText(text, resultFromPDF, footNotes, false);
                else if (tmpUncommitedRef != "") // means there is some text that's a reference
                    throw new Exception(); // illegal shouldn't happen

                isFootNote = true;

                // this logic is ok dont modify
                if (tmpUncommitedRef != "" && resultFromPDF.Remove(0, 1) == tmpUncommitedRef.Remove(tmpUncommitedRef.Length - 1, 1))
                    return 0;
                
                ProcessReferenceText(text, resultFromPDF, footNotes, false);

                return 0;
            }

            if (size == 6 || size == 7) // no need to further processing as this is just a reference detail
            {
                //var splittedRef = text.Range.Text.Split(new[] { "¿" }, StringSplitOptions.None);􀀍 or ,
                var splittedRef = text.Range.Text.Split(new[] { "􀀍" }, StringSplitOptions.None);
                linesToSkip = splittedRef.Length;

                if (isFootNote)
                {
                    if (linesToSkip <= 1) // this line is also part of the foot-note
                    {
                        ProcessReferenceText(text, resultFromPDF, footNotes, false);
                        return 0;
                    }

                    ProcessReferenceText(text, resultFromPDF, footNotes, true);
                    isFootNote = false;
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

                return linesToSkip - 1;
            }

            //page title hence just skip this line and continue . . . 
            if (size == 10 || size == 11 || size == 14) // pageer/page # hence references start . . . 
            {
                linesToSkip = text.Range.Text.Split(new[] { "􀀍" }, StringSplitOptions.None).Length;

                if (isFootNote)
                {
                    ProcessReferenceText(text, resultFromPDF, footNotes, true);
                    isFootNote = false;
                }

                if (tmpUncommitedRef != "")
                {
                    ProcessReferenceText(text, resultFromPDF, footNotes, true);
                    tmpUncommitedRef = "";
                }

                // Process remaining reference details if exists...
                if (refDetails.Count > 0)
                {
                    InsertRefDetails(refDetails, chapter);
                }

                return linesToSkip - 1;
            }

            throw new Exception();//illegal shouldn't happen
        }

        private void ProcessReferenceText(Paragraph text, string resultFromPDF, Dictionary<int, string> footNotes, bool isFinal)
        {
            string font = "";
            string tmpRef = "";

            resultFromPDF = resultFromPDF.Remove(0, 1);

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
                    if (resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length).Length == tmpRef.Length)
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);
                    else // means there is a problem??????? TODO: what will happen . . . .                    
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);

                    tmpUncommitedRef += tmpRef;
                    //if(tmpUncommitedRefFontChangeCharacter == "")
                    //    throw  new Exception();

                    tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRefFontChangeCharacter = (tmpUncommitedRefFontChangeCharacter == "" ? tmpUncommitedRef[0].ToString() : tmpUncommitedRefFontChangeCharacter);
                    resultFromPDF = resultFromPDF.Remove(0, tmpRef.Length);
                    tmpUncommitedRef = "";
                    tmpRef = "";
                }

                if (character.Font.Size == 3.5 && (tmpUncommitedRef != "" || tmpRef != "")) // means there is another foot-note here
                {
                    if (resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length).Length == tmpRef.Length)
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);
                    else // means there is a problem??????? TODO: what will happen . . . .                    
                        tmpRef = resultFromPDF.Remove(tmpRef.Length, resultFromPDF.Length - tmpRef.Length);

                    tmpUncommitedRef += tmpRef;
                    tmpVerseId = GetVerseRef(footNotes, tmpUncommitedRefFontChangeCharacter != "" ?
                                                                                tmpUncommitedRefFontChangeCharacter : tmpUncommitedRef[0].ToString());

                    if (tmpVerseId == 0)
                        throw new Exception();

                    InsertReference(GetChapterId(tmpVerseId), tmpVerseId, ++tmpSequence, tmpUncommitedRef, font, (int)RefType.FOOTNOTE); // verse b4 z foot-note character
                    tmpUncommitedRef = "";
                    tmpUncommitedRefFontChangeCharacter = "";
                    resultFromPDF = resultFromPDF.Remove(0, tmpRef.Length);
                    footNotes.Remove(tmpVerseId);
                    tmpSequence = 0;
                    tmpRef = "";
                }

                tmpRef += character.Text;
                font = character.Font.Name;
            }

            if (isFootNote)
            {
                tmpRef = resultFromPDF + "\r";
            }
            else
            {
                if (resultFromPDF.Remove(tmpUncommitedRef.Length, resultFromPDF.Length - tmpUncommitedRef.Length).Length == tmpUncommitedRef.Length)
                    tmpRef = resultFromPDF + "\r";
                else // means there is a problem??????? TODO: what will happen . . . .
                    throw new Exception();
            }

            tmpUncommitedRef += tmpRef;
        }

        private string ReadPDFRecord(int prevRecord, out int nextRecord, int skipedRecords)
        {
            return SqlMgr.ReadPDFRecord(prevRecord, out nextRecord, skipedRecords);
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
                    Verse = new Verse{No = reference.VerseNo},
                    Sequene = reference.Sequence,
                    Text = reference.Text,
                    Font = new Font { Name = reference.FontName },
                    RefText = reference.Text[0].ToString(),
                    Type = (RefType)reference.Type
                });
            }

            return chapterReferences;
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
            var split = refDetails.First.Value.Split(new[] { "􀀍" }, StringSplitOptions.None); // chapter & verse #

            int tmpVerseId = GetVerseId(chapter.Book.Id, Convert.ToInt32(split[0]), Convert.ToInt32(split[1]));
            int tmpChapterId = GetChapterId(tmpVerseId);

            string tmpVerseDetail = "";
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