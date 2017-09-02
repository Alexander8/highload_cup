namespace Travels.Data.Model
{
    internal sealed class Visit
    {
        public long id { get; set; }

        public long location { get; set; }

        public long user { get; set; }

        public long visited_at { get; set; }

        public long mark { get; set; }
    }
}
