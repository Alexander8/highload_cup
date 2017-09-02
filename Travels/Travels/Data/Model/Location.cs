namespace Travels.Data.Model
{
    internal sealed class Location
    {
        public long id { get; set; }

        public string place { get; set; }

        public string country { get; set; }

        public string city { get; set; }

        public long distance { get; set; }
    }
}
