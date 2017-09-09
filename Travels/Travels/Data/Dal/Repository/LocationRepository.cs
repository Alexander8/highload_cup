using System;
using System.Linq;
using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class LocationRepository
    {
        private static readonly Visit EmptyVisit = new Visit(-1, -1, -1, 0, 0);

        public static Location GetLocation(int id)
        {
            return id < Storage.Locations.Length - 1 ? Storage.Locations[id] : null;
        }

        public static bool LocationExists(int id)
        {
            return id < Storage.Locations.Length - 1 && Storage.Locations[id] != null;
        }

        public static double GetAverageLocationMark(int id, long? fromDate, long? toDate, int? fromAge, int? toAge, string gender)
        {
            var location = Storage.Locations[id];

            if (location.Visits == null)
                return 0d;

            var query = location.Visits.AsEnumerable();
            if (fromDate.HasValue)
                query = query.Where(v => v.VisitedAt > fromDate);

            if (toDate.HasValue)
                query = query.Where(v => v.VisitedAt < toDate);

            if (fromAge.HasValue)
                query = query.Where(v => v.User.Age >= fromAge.Value);

            if (toAge.HasValue)
                query = query.Where(v => v.User.Age < toAge.Value);

            if (gender != null)
                query = query.Where(v => v.User.Gender == gender);         

            var avg = query.DefaultIfEmpty(EmptyVisit).Average(v => v.Mark);
            return Math.Round(avg, 5);
        }
    }
}
