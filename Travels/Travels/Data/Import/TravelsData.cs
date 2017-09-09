using System.Collections.Generic;
using Travels.Data.Model;

namespace Travels.Data.Import
{
    internal sealed class TravelsData
    {
        public readonly List<User> Users = new List<User>();
        public readonly List<Location> Locations = new List<Location>();
        public readonly List<Visit> Visits = new List<Visit>();
        public long CurrentTimestamp { get; set; }
    }
}
