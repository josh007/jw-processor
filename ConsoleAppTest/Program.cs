using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibleDataLayer;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            Bible bible = new Bible(ConnectionString: "Data Source=joshdb.sqlite;Version=3;foreign keys=true;");
            //bible.PopulateTestData();
            //return;
            //bible.CreateBible();
           bible.BibleParser(fileName: @"E:\share\joshua.docx", bookName: "joshua");

            var xx = bible.GetVerse("joshua", 1, 1);
            string result = "";
            foreach (var verse in xx)
            {
                if (result == "")
                    result = string.Format("Book: {0} Chapter: {1}, Verse: {2}", verse.Chapter.Book.Name, verse.Chapter.ChapterNo, verse.No);

                result += string.Format("SEQ:{0} TXT:{1} FNT:{2}({3})", verse.Sequene, verse.Text, verse.Font.Name, verse.Size);
            }
        }
    }
}
