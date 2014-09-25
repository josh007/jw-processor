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
            REF =3
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

            string source =  stripper.getText(doc);
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
                else if (i == 1  && lines[i+1].Trim().Replace("\n", "") == "") // means this is also a heading
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

            lines = source2.Split(new[]{"\r"},StringSplitOptions.None);

            for (int i = 0; i < lines.Count(); i++)
            {
                if (lines[i].Trim().Replace("\n", "") == "") // means next line could be potentially a heading
                {
                    InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line

                    if ((i + 2) < lines.Count() && lines[i + 2].Trim().Replace("\n", "") == "") // definetely a heading
                    {
                        tmp = lines[i + 1].Replace("\n", "").Trim();
                        InsertBookDetails(pdf_book_id, tmp, (tmp == "" ? RefType.NONE:RefType.HEADING)); // the line next to the one above
                    }
                    InsertBookDetails(pdf_book_id, "", RefType.NONE); // current line
                    i += 2; // skip two cause i already processed them . .. 
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
    }
}
