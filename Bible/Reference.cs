namespace BibleDataLayer
{
    public class Reference
    {
        public int Id { get; set; }
        public Chapter Chapter { get; set; }
        public Verse Verse { get; set; }

        public Font Font { get; set; }

        public int Sequene { get; set; }
        public string RefText { get; set; }
        public string Text { get; set; }

        public Bible.RefType Type { get; set; }
    }
}