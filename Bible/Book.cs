using System.Collections.Generic;

namespace BibleDataLayer
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Chapter> Chapters { get; set; }

        public Book()
        {
            Chapters = new List<Chapter>();
        }
        public string GetWholeBook()
        {
            return null;
        }

        public string GetChapter(int chapter)
        {
            return null;
        }
    }
}