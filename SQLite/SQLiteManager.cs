using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SQLite
{
    public class SQLiteManager
    {
        public SQLiteConnection Connection;
        public SQLiteCommand Command;

        public SQLiteManager(string connectionString)// = "Data Source=joshdb.sqlite;Version=3;foreign keys=true;")
        {
            Connection = new SQLiteConnection(connectionString);
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Command = new SQLiteCommand(Connection);
        }

        public void CreateTableStructures()
        {
            Connection.Close();
            File.Delete("joshdb.sqlite");
            Connection.Open();

            string sql = "CREATE TABLE fonts(id INTEGER PRIMARY KEY AUTOINCREMENT, font_name VARCHAR(50))";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();


            sql = "CREATE TABLE books(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(50))";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "CREATE TABLE chapters(id INTEGER PRIMARY KEY AUTOINCREMENT, book_id INTEGER, chapter_no INTEGER, " +
                  "FOREIGN KEY (book_id) REFERENCES books(id) ON DELETE CASCADE)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "CREATE TABLE verses(id INTEGER PRIMARY KEY AUTOINCREMENT, chapter_id INTEGER, verse_no INTEGER, sequence INTEGER, " +
                  "verse_text VARCHAR(255), font_id INTEGER, font_size REAL, " +
                  "FOREIGN KEY (chapter_id) REFERENCES chapters(id) ON DELETE CASCADE)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "CREATE TABLE 'references'(id INTEGER PRIMARY KEY  AUTOINCREMENT, chapter_id INTEGER, verse_id INTEGER, text VARCHAR(255), " +
                  "font_id INTEGER, sequence INTEGER, type INTEGER, FOREIGN KEY (chapter_id) REFERENCES chapters(id) ON DELETE CASCADE, " +
                  "FOREIGN KEY (verse_id) REFERENCES verses(id) ON DELETE CASCADE)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();


            sql = "CREATE TABLE reference_exceptions(id INTEGER PRIMARY KEY  AUTOINCREMENT,book_id INTEGER ,chapter_no INTEGER, " +
                  "verse_no INTEGER, text VARCHAR(255), FOREIGN KEY (book_id) REFERENCES books(id) ON DELETE CASCADE)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "CREATE VIEW vw_references AS " +
                  "SELECT ref.*, chapter_no,verse_no, name FROM 'references' ref " +
                  "INNER JOIN chapters ON ref.chapter_id = chapters.id " +
                  "INNER JOIN verses ON ref.verse_id = verses.id " +
                  "INNER JOIN books ON chapters.book_id = books.id " +
                  "INNER JOIN fonts ON ref.font_id = fonts.id " +
                  "ORDER BY name, chapter_no, verse_no, sequence";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();


            sql = "CREATE VIEW vw_book_verse_row AS " +
                    "SELECT books.id AS book_id, books.name AS book_name,chapters.id AS chapter_id,chapters.chapter_no, " +
                      "verses.id AS verse_id, verses.verse_no, verses.sequence, verses.verse_text, " +
                        "fonts.id AS font_id, fonts.font_name, verses.font_size FROM books " +
                            "INNER JOIN chapters ON books.id = chapters.book_id " +
                                "INNER JOIN verses ON chapters.id = verses.chapter_id " +
                                    "INNER JOIN fonts ON verses.font_id = fonts.id";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            // for text processing data would b imported in these tables to which the text would be extracted
            sql = "CREATE TABLE pdfbooks(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(50))";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            // ref_type is 0: Normal text 1: HEADING 2:FOOTNOTE 3:REF
            sql = "CREATE TABLE pdfbookdetails(id INTEGER PRIMARY KEY AUTOINCREMENT, pdf_book_id INTEGER, " +
                  "text VARCHAR(255), ref_type INTEGER, FOREIGN KEY (pdf_book_id) REFERENCES pdfbooks(id) ON DELETE CASCADE)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "CREATE VIEW vw_pdf_book_detail_row AS " +
                    "SELECT pdfbooks.id AS book_id, pdfbooks.name AS pdf_book_name, " +
                        "pdfbookdetails.id AS pdfbooks_detail_id, text, ref_type " +
                            "FROM pdfbooks INNER JOIN pdfbookdetails " +
                                "ON pdfbooks.id = pdfbookdetails.pdf_book_id ";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            //sql = "CREATE TABLE verse_details(id INTEGER PRIMARY KEY AUTOINCREMENT, sequence INTEGER, verse_id INTEGER, font_id INTEGER," +
            //      "FOREIGN KEY (verse_id) REFERENCES verses(id) ON DELETE CASCADE)";
            //Command.CommandText = sql;
            //Command.ExecuteNonQuery();

        }

        public void PopulateTestData()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            //Command = new SQLiteCommand(Connection);

            string sql = "INSERT INTO fonts(font_name)VALUES('Times New Roman')";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            int font_id = Convert.ToInt32(Command.ExecuteScalar());


            sql = "INSERT INTO books(name)VALUES('joshua')";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            int book_id = Convert.ToInt32(Command.ExecuteScalar());

            sql = "INSERT INTO chapters(book_id, chapter_no)VALUES(" + book_id + ", 1)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            int chapter_id = Convert.ToInt32(Command.ExecuteScalar());

            sql = "INSERT INTO verses(chapter_id, verse_no, sequence, verse_text, font_id, font_size)VALUES(" +
                  chapter_id + ", 1,1,'this is z verse'," + font_id + ",5.5)";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            sql = "SELECT books.id AS book_id, books.name AS book_name,chapters.id AS chapter_id,chapters.chapter_no, " +
                  "verses.id AS verse_id, verses.verse_no, verses.sequence, verses.verse_text, " +
                    "fonts.font_name, verses.font_size FROM books " +
                        "INNER JOIN chapters ON books.id = chapters.book_id " +
                            "INNER JOIN verses ON chapters.id = verses.chapter_id " +
                                "INNER JOIN fonts ON verses.font_id = fonts.id";
            sql = "SELECT * FROM vw_book_verse_row ";
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            string output = "";
            while (reader.Read())
            {
                output += string.Format("books.id : {0}, books.name: {1}, chapters.id: {2}, chapters.chapter_no: {3}",
                                  reader[0], reader[1], reader[2], reader[3]);
                output += Environment.NewLine;
            }
            reader.Close();

        }

        //public void CreateDataBase()
        //{
        //    SQLiteConnection.CreateFile("joshdb.sqlite");
        //}

        public void InsertReferenceException(int book_id, int chapter_no, int verse_no, string ref_text)
        {
            string sql = "INSERT INTO reference_exceptions(book_id, chapter_no, verse_no, text)VALUES(" +
                                            book_id + ", " + chapter_no + "," + verse_no + ",'" + ref_text + "')";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        public void InsertReference(int chapter_id, int verse_id, int sequence, string ref_text, string font_name, int type)
        {
            string sql = "SELECT Id FROM fonts WHERE font_name LIKE '%" + font_name + "%' LIMIT 1";
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            int font_id = 0;
            while (reader.Read())
            {
                font_id = Convert.ToInt32(reader[0]);
            }
            reader.Close();

            // means no font insert the new font
            if (font_id == 0)
            {
                sql = "INSERT INTO fonts(font_name)VALUES('" + font_name + "')";
                Command.CommandText = sql;
                Command.ExecuteNonQuery();

                Command.CommandText = "SELECT last_insert_rowid()";
                font_id = Convert.ToInt32(Command.ExecuteScalar());
            }


            sql = "INSERT INTO 'references'(chapter_id, verse_id, sequence, text, font_id, type)VALUES(" +
                                            chapter_id + "," + verse_id + "," + sequence + ",'" +
                                            ref_text + "', " + font_id + ", " + type + ")";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        public int InsertVerse(int chapter_id, int verse_no, int sequence, string verse_text, string font_name, double font_size)
        {
            string sql = "SELECT Id FROM fonts WHERE font_name LIKE '%" + font_name + "%' LIMIT 1";
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            int font_id = 0;
            while (reader.Read())
            {
                font_id = Convert.ToInt32(reader[0]);
            }
            reader.Close();

            // means no font insert the new font
            if (font_id == 0)
            {
                sql = "INSERT INTO fonts(font_name)VALUES('" + font_name + "')";
                Command.CommandText = sql;
                Command.ExecuteNonQuery();

                Command.CommandText = "SELECT last_insert_rowid()";
                font_id = Convert.ToInt32(Command.ExecuteScalar());
            }


            sql = "INSERT INTO verses(chapter_id, verse_no, sequence, verse_text, font_id, font_size)VALUES(" +
                                            chapter_id + "," + verse_no + "," + sequence + ",'" +
                                            verse_text + "', " + font_id + ", " + font_size + ")";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public int InsertChapter(int book_id, int chapter_no)
        {
            string sql = "INSERT INTO chapters(book_id, chapter_no)VALUES(" +
                                    book_id + "," + chapter_no + ")";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(Command.ExecuteScalar());

        }

        public int InsertBook(string book_name)
        {
            string sql = "INSERT INTO books(name)VALUES('" + book_name + "')";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();

            Command.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public Dictionary<int, List<VerseResult>> GetChapters(string book_name)
        {
            var result = new Dictionary<int, List<VerseResult>>();

            string sql = "SELECT chapter_no, verse_no, sequence,verse_text, font_name, font_size " +
                            "FROM vw_book_verse_row WHERE book_name = '" + book_name + "'";
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            int chapterNo = 1;
            var currentChapter = new List<VerseResult>();

            while (reader.Read())
            {
                if (chapterNo != Convert.ToInt32(reader[0]))
                {
                    result.Add(chapterNo, currentChapter);
                    currentChapter = new List<VerseResult>();
                    chapterNo = Convert.ToInt32(reader[0]);
                }

                currentChapter.Add(new VerseResult
                    {
                        VerseNo = Convert.ToInt32(reader[1]),
                        Sequence = Convert.ToInt32(reader[2]),
                        Text = reader[3].ToString(),
                        FontName = reader[4].ToString(),
                        Size = Convert.ToDouble(reader[5])
                    });

            }

            reader.Close();

            if (currentChapter.Count > 0)
                result.Add(chapterNo, currentChapter);

            return result;
        }

        public List<VerseResult> GetChapter(string book_name, int chapter_no)
        {
            var result = new List<VerseResult>();

            string sql = "SELECT verse_no, sequence, verse_text, font_name, font_size " +
                            "FROM vw_book_verse_row WHERE book_name = '" + book_name + "' AND chapter_no = " + chapter_no;
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(
                    new VerseResult
                    {
                        VerseNo = Convert.ToInt32(reader[0]),
                        Sequence = Convert.ToInt32(reader[1]),
                        Text = reader[2].ToString(),
                        FontName = reader[3].ToString(),
                        Size = Convert.ToDouble(reader[4])
                    });
            }

            reader.Close();

            return result;
        }

        public List<VerseResult> GetVerse(string book_name, int chapter_no, int verse_start, int verse_end)
        {
            var result = new List<VerseResult>();

            string sql = "SELECT verse_no, sequence, verse_text, font_name, font_size " +
                            "FROM vw_book_verse_row WHERE book_name = '" + book_name +
                                "' AND chapter_no = " + chapter_no +
                                    " AND verse_no BETWEEN " + verse_start + " AND " + verse_end;
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(
                    new VerseResult
                    {
                        VerseNo = Convert.ToInt32(reader[0]),
                        Sequence = Convert.ToInt32(reader[1]),
                        Text = reader[2].ToString(),
                        FontName = reader[3].ToString(),
                        Size = Convert.ToDouble(reader[4])
                    });
            }

            reader.Close();

            return result;
        }

        public void TruncateBibleInfoFromDb(string book_name)
        {
            string sql = "DELETE FROM books WHERE name = '" + book_name + "'";
            Command.CommandText = sql;
            Command.ExecuteNonQuery();
        }

        public int GetNoOfChapters(string book_name)
        {
            string sql = "SELECT MAX(chapter_no) FROM vw_book_verse_row WHERE book_name = '" + book_name + "'";
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public int GetNoOfVerses(string book_name, int chapter_no)
        {
            string sql = "SELECT MAX(verse_no) FROM vw_book_verse_row WHERE book_name = '" + book_name + "' AND chapter_no = " + chapter_no;
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public int ReadFirstPDFRecord(string book_name)
        {
            string sql = "SELECT MIN(pdfbooks_detail_id) FROM vw_pdf_book_detail_row WHERE pdf_book_name = '" + book_name + "'";
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public string ReadPDFRecord(int prev_record, out int next_record, int skiped_records)
        {
            string result = "";
            next_record = prev_record;
            string sql = "SELECT pdfbooks_detail_id, text, ref_type FROM vw_pdf_book_detail_row " +
                         "WHERE pdfbooks_detail_id > " + prev_record + " ORDER BY pdfbooks_detail_id ASC";
            Command.CommandText = sql;

            SQLiteDataReader reader = Command.ExecuteReader();
            while (reader.Read())
            {
                if (skiped_records != 0)
                {
                    skiped_records--;
                    continue;
                }
                result = reader[1].ToString().Trim();// ck 2 c if empty then skip
                if (result != "") // 
                {
                    next_record = Convert.ToInt32(reader[0]);
                    result = reader[2] + result;
                    break;
                }
            }

            reader.Close();
            
            return result;
        }

        public int GetChapterId(int verse_id)
        {
            string sql = "SELECT chapter_id FROM vw_book_verse_row WHERE verse_id = " + verse_id;
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public int GetVerseId(int book_id, int chapter_no, int verse_no)
        {
            string sql = "SELECT verse_id FROM vw_book_verse_row WHERE book_id = " + book_id +
                            " AND chapter_no = " + chapter_no + " AND verse_no = " + verse_no;
            Command.CommandText = sql;
            return Convert.ToInt32(Command.ExecuteScalar());
        }

        public List<ReferenceResult> GetChapterReferences(string book_name, int chapter_no)
        {
            var result = new List<ReferenceResult>();

            string sql = "SELECT chapter_no,verse_no, font_name, text, sequence, type " +
                            "FROM vw_references WHERE book_name = '" + book_name + "' AND chapter_no = " + chapter_no;
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(
                    new ReferenceResult
                    {
                        ChapterNo = Convert.ToInt32(reader[0]),
                        VerseNo = Convert.ToInt32(reader[1]),
                        FontName = reader[2].ToString(),
                        Text = reader[3].ToString(),
                        Sequence = Convert.ToInt32(reader[4]),
                        Type = Convert.ToInt32(reader[5])
                    });
            }

            reader.Close();

            return result;
        }

        public Dictionary<int, List<ReferenceResult>> GetReferences(string book_name)
        {
            var result = new Dictionary<int, List<ReferenceResult>>();

            string sql = "SELECT chapter_no, verse_no, font_name, text, sequence, type " +
                            "FROM vw_references WHERE name = '" + book_name + "'";
            Command.CommandText = sql;
            SQLiteDataReader reader = Command.ExecuteReader();

            int chapterNo = 0;
            var currentChapter = new List<ReferenceResult>();

            while (reader.Read())
            {
                if (chapterNo != Convert.ToInt32(reader[0]))
                {
                    chapterNo = Convert.ToInt32(reader[0]);
                    currentChapter = new List<ReferenceResult>();
                    result.Add(chapterNo, currentChapter);
                }

                currentChapter.Add(new ReferenceResult
                {
                    ChapterNo = Convert.ToInt32(reader[0]),
                    VerseNo = Convert.ToInt32(reader[1]),
                    FontName = reader[2].ToString(),
                    Text = reader[3].ToString(),
                    Sequence = Convert.ToInt32(reader[4]),
                    Type = Convert.ToInt32(reader[5])
                });

            }

            reader.Close();

            return result;
        }
    }
}