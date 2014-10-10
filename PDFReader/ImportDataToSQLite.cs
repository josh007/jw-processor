using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;

namespace PDFReader
{
    public class ImportDataToSQLite
    {
        internal SQLiteConnection Connection;
        internal SQLiteCommand Command;
        internal bool isFootNoteStarted = false;

        public ImportDataToSQLite()
        {
            Connection = new SQLiteConnection("Data Source=joshdb.sqlite;Version=3;foreign keys=true;");
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Command = new SQLiteCommand(Connection);
        }

        public enum RefType
        {
            NONE = 0,
            HEADING = 1,
            FOOTNOTE = 2,
            REF = 3
        }

        public string Import(string fileName, string bookName)
        {
            string sql = "DELETE FROM pdfbooks WHERE name = '" + bookName + "'";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "INSERT INTO pdfbooks(name)VALUES('" + bookName + "')";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            int pdf_book_id = Convert.ToInt32(Command.ExecuteScalar());

            // NOW import all the read text to . . . parse using PDFBox
            PDDocument doc = PDDocument.load(fileName);
            PDFTextStripper stripper = new PDFTextStripper();


            // Process 1st  page as its' special
            stripper.setStartPage(1);
            stripper.setEndPage(1);

            string source = stripper.getText(doc);
            var lines = source.Split(new[] { "\r" }, StringSplitOptions.None);
            string tmp = "";

            for (int i = 0; i < lines.Count(); i++)
            {
                if (i == 0) // means first line
                {
                    tmp = lines[i].Replace("\n", "");
                    if (!char.IsDigit(tmp[0])) // if number that's the begining of a chapter otherwise it's a heading
                        InsertBookDetails(pdf_book_id, tmp, RefType.HEADING);
                }
                else if (i == 1 && lines[i + 1].Trim().Replace("\n", "") == "") // means this is also a heading
                {
                    tmp = lines[i].Replace("\n", "");
                    InsertBookDetails(pdf_book_id, tmp, RefType.HEADING);
                }
                else if (lines[i].Trim().Replace("\n", "") == "") // means next line could be potentially a heading
                {
                    InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line

                    if ((i + 2) < lines.Count() && lines[i + 2].Trim().Replace("\n", "") == "") // definetely a heading
                    {
                        tmp = lines[i + 1].Replace("\n", "");
                        InsertBookDetails(pdf_book_id, tmp, RefType.HEADING); // the line next to the one above
                        InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line
                        i += 2; // skip two cause i already processed them . .. 
                    }
                }
                else // means normal text
                {
                    tmp = lines[i].Replace("\n", "");
                    InsertBookDetails(pdf_book_id, tmp, RefType.NONE); // the line next to the one above
                }
            }

            // Process all other pages 
            stripper.setStartPage(2);
            stripper.setEndPage(5000);

            string source2 = stripper.getText(doc);

            lines = source2.Split(new[] { "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i].Trim().Replace("\n", "") == "") // means next line could be potentially a heading
                {
                    InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line

                    if ((i + 2) < lines.Count() && lines[i + 2].Trim().Replace("\n", "") == "") // definetely a heading
                    {
                        tmp = lines[i + 1].Replace("\n", "").Trim();
                        InsertBookDetails(pdf_book_id, tmp, (tmp == "" ? RefType.NONE : RefType.HEADING)); // the line next to the one above
                        InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line
                        i += 2; // skip two cause i already processed them . .. 
                    }
                }
                else // means normal text
                {
                    tmp = lines[i].Replace("\n", "");
                    InsertBookDetails(pdf_book_id, tmp, RefType.NONE); // the line next to the one above
                }
            }

            return source + source2;
        }

        private void InsertBookDetails(int pdf_book_id, string txt, RefType referenceType)
        {
            string sql;
            sql = "INSERT INTO pdfbookdetails(pdf_book_id, text, ref_type)VALUES(" // 0: HEADING 1:FOOTNOTE 2:REF
                  + pdf_book_id + ", '" + txt + "'," + (int)referenceType + ")";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        private void InsertVerse(int pdf_book_id, int chapter_no, int verse_no, string txt, RefType type)
        {
            string sql;
            sql = "INSERT INTO pdfbooks_fixed(pdf_book_id, chapter_no, verse_no, text, type)VALUES(" // 0: NORMAL 1: HEADING 2:FOOTNOTE 3:REF
                  + pdf_book_id + "," + chapter_no + "," + verse_no + ", '" + txt + "'," + (int)type + ")";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        private int GetBookId(string bookName)
        {
            string sql;
            sql = "SELECT Id FROM pdfbooks WHERE name = '" + bookName + "'";
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        private bool ValidateRecordIfVerse(string result, int currentChapter, int currentVerse)
        {
            if (result.Length <= 3)
                return false;

            if (result[0] == '1')
                return true;

            result = result.Remove(0, 1).Trim();
            //if (char.IsLetter(result[0]) || char.IsPunctuation(result[0]))
            if (!char.IsDigit(result[0]))
            {
                if (result.Contains(" "))
                {
                    var split = result.Split(new[] { " " }, StringSplitOptions.None);

                    if (split.Length > 2 && split[2].Trim() != "")
                        return true;

                    if (split[1].Length < 2 || char.IsDigit(split[1][0]))
                        return false;

                    return true;
                }

                if (!isFootNoteStarted)
                    return true;

                return false;
            }


            var tmp = "";
            char chr;

            for (int i = 0; i < result.Length; i++)
            {
                chr = result[i];

                if (chr == ' ')
                    continue;

                if (char.IsDigit(chr))
                {
                    tmp += chr;
                    continue;
                }

                if (!char.IsDigit(chr) && tmp != "")
                {
                    result = result.Remove(0, i+1);
                    if (result.Contains(" "))
                    {
                        if (tmp != currentChapter.ToString() && tmp != currentVerse.ToString())
                            return false;

                        var split = result.Split(new[] { " " }, StringSplitOptions.None);
                        if (split.Length > 2 && split[2].Trim() != "")
                            return true;
                        
                        return false;
                    }

                    return false;
                }
            }

            return false;
        }

        private void InitializeFixBook(int pdf_book_id)
        {
            string sql;
            sql = "DELETE FROM pdfbooks_fixed WHERE pdf_book_id =" + pdf_book_id;
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        public Dictionary<int, int> MapBookVerses(string bookName)
        {
            var bookId = GetBookId(bookName);
            InitializeFixBook(bookId);

            var manager = new SQLiteManager("Data Source=joshdb.sqlite;Version=3;foreign keys=true;");
            Dictionary<int, int> index = new Dictionary<int, int>();

            var chapters = manager.GetNoOfChapters(bookName);

            for (int i = 1; i <= chapters; i++)
            {
                var verses = manager.GetNoOfVerses(bookName, i);
                index.Add(i, verses);
            }
            
            int currentChapter = 1;
            int currentVerse = 2;
            bool isChapter = false;
            bool isFirstChapterFound = true;
            bool shouldChapterNumberRemoved = false;
            bool isPotentialFootNote = false;
            string potentialFootNoteText = "";

            string result = null;
            string text = "";

            var prev_rec = manager.ReadFirstPDFRecord(bookName) - 1;

            do
            {
                result = manager.ReadPDFRecord(prev_rec, out prev_rec);

                if (currentChapter == 3 && currentVerse == 16)
                    break;

                if (result.Remove(0,1).Trim() == "")
                {
                    isPotentialFootNote = true;
                    continue;
                    // potential foot-note start 
                    // potential header
                    // potential chapter 
                }

                if (!ValidateRecordIfVerse(result, currentChapter, currentVerse))
                {
                    isFootNoteStarted = true;
                    potentialFootNoteText = "";
                    continue;
                }

                if (index[currentChapter] + 1 == currentVerse) // means we is looking a chapter now
                    isChapter = true;

                if (currentChapter == 1 && result[0] == '1' && currentVerse == 2) // first chapter heading
                {
                    InsertVerse(bookId, 1, 1, result.Remove(0, 1), RefType.HEADING); // first chp 1st vrs 1st heading
                    continue;
                }

                
                if (result[0] == '1')
                {
                    #region 

                    if (isChapter) // chapter header
                    {
                        isFootNoteStarted = false;

                        if (isPotentialFootNote)
                        {
                            text += potentialFootNoteText;
                            isPotentialFootNote = false;
                        }

                        if (text.Trim() != "")
                            InsertVerse(bookId, currentChapter, currentVerse - 1, text, RefType.NONE); // prev verse

                        InsertVerse(bookId, currentChapter + 1, 1, result.Remove(0, 1), RefType.HEADING);
                        currentChapter++;
                        currentVerse = 2;
                        isChapter = false;
                        shouldChapterNumberRemoved = true;
                        text = "";

                        continue;
                    }

                    if (text.Trim() != "" && !isPotentialFootNote)
                    {
                        InsertVerse(bookId, currentChapter, currentVerse - 1, text, RefType.NONE); // prev verse
                        text = "";
                    }

                    // inbetween titles . . . 
                    if (!isFootNoteStarted)
                        InsertVerse(bookId, currentChapter, currentVerse, result.Remove(0, 1), RefType.HEADING);

                    continue; 

                    #endregion
                }

                result = result.Remove(0, 1);

                if(!isPotentialFootNote)
                    isFootNoteStarted = false;

                if (isChapter) // no chapter header
                {
                    #region

                    if (!result.Contains(currentChapter.ToString()))
                    {
                        if (isPotentialFootNote)
                        {
                            text += potentialFootNoteText;
                            isPotentialFootNote = false;
                            potentialFootNoteText = "";
                        }
                        text += result;
                    }
                    else
                    {
                        if (text.Trim() != "")
                            InsertVerse(bookId, currentChapter, currentVerse, text, RefType.NONE); // prev verse

                        currentChapter++;
                        currentVerse = 2;
                        isChapter = false;
                        shouldChapterNumberRemoved = true;
                        text = "";

                        result = result.Remove(0, currentChapter.ToString().Length);
                        if (!result.Contains(currentVerse.ToString()))
                        {
                            text += result;
                            continue;
                        }

                        int found = result.IndexOf(currentVerse.ToString());
                        text += result.Substring(0, found);
                        InsertVerse(bookId, currentChapter, currentVerse - 1, text, RefType.NONE);

                        text = result.Substring(found, result.Length - found);
                        currentVerse++;
                    }

                    continue; 

                    #endregion
                }

                if (isFirstChapterFound) // first chapter # should b removed if it has no heading
                {
                    result = result.Remove(0, 1);
                    isFirstChapterFound = false;
                }

                if (shouldChapterNumberRemoved)
                {
                    result = result.Remove(0, currentChapter.ToString().Length);
                    shouldChapterNumberRemoved = false;
                }


                if (!result.Contains(currentVerse.ToString()))
                {
                    if (isPotentialFootNote)
                        potentialFootNoteText += result;
                    else
                        text += result;
                }
                else
                {
                    int found = result.IndexOf(currentVerse.ToString());

                    // false positive ...
                    if (char.IsDigit(result[found + currentVerse.ToString().Length]) ||
                        result[found + currentVerse.ToString().Length] == ' ')
                    {
                        if (isPotentialFootNote)
                            potentialFootNoteText += result;
                        else
                            text += result;
                        continue;
                    }

                    if (isPotentialFootNote)
                    {
                        text += potentialFootNoteText;
                        isPotentialFootNote = false;
                        potentialFootNoteText = "";
                    }

                    text += result.Substring(0, found);
                    InsertVerse(bookId, currentChapter, currentVerse - 1, text, RefType.NONE);

                    text = result.Substring(found + (currentVerse.ToString().Length), result.Length - found - (currentVerse.ToString().Length));
                    currentVerse++;
                }

            } while (true);

            return index;
        }   
    }
}
