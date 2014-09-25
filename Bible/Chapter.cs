using System.Collections.Generic;

namespace BibleDataLayer
{
    public class Chapter
    {
        public int Id { get; set; }
        public Book Book { get; set; }
        public int ChapterNo { get; set; }
        public List<Verse> Verses { get; set; }

        public Chapter()
        {
            Verses = new List<Verse>();
        }

        public string GetWholeChapter()
        {
            return null;
        }

        public string GetVerse(int[] verse)
        {
            return null;
        }
    }
}