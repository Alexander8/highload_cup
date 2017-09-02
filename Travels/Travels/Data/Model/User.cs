namespace Travels.Data.Model
{
    internal sealed class User
    {
        public long id { get; set; }

        public string email { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public long gender { get; set; }

        public long birth_date { get; set; }
    }
}
