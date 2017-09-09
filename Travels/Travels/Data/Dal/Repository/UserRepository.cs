using System.Collections.Generic;
using System.Linq;
using Travels.Data.Dto;
using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class UserRepository
    {
        public static User GetUser(int id)
        {
            return id < Storage.Users.Length - 1 ? Storage.Users[id] : null;
        }

        public static bool UserExists(int id)
        {
            return id < Storage.Users.Length - 1 && Storage.Users[id] != null;
        }

        public static IEnumerable<UserVisitToLocationDto> GetUserVisits(int id, long? fromDate, long? toDate, string country, int? toDistance)
        {
            var user = Storage.Users[id];

            if (user.Visits == null)
                return new UserVisitToLocationDto[0];

            var query = user.Visits.AsEnumerable();

            if (fromDate.HasValue)
                query = query.Where(v => v.VisitedAt > fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(v => v.VisitedAt < toDate.Value);

            if (country != null)
                query = query.Where(v => v.Location.Country == country);

            if (toDistance.HasValue)
                query = query.Where(v => v.Location.Distance < toDistance.Value);

            return query
                .OrderBy(v => v.VisitedAt)
                .Select(v => new UserVisitToLocationDto
                {
                    place = v.Location.Place,
                    mark = v.Mark,
                    visited_at = v.VisitedAt
                });    
        }
    }
}
