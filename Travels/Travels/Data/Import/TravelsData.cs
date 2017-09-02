using System.Collections.Generic;

namespace Travels.Data.Import
{
    internal sealed class TravelsData
    {
        public List<UserData> Users { get; set; }

        public List<LocationData> Locations { get; set; }

        public List<VisitData> Visits { get; set; }
    }
}
