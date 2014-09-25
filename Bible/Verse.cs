namespace BibleDataLayer
{
    public class Verse
    {
        public int Id { get; set; }
        public Chapter Chapter { get; set; }
        public Font Font { get; set; }

        public int Sequene { get; set; }
        public int No { get; set; }
        public string Text { get; set; }
        public double Size { get; set; }
    }
}