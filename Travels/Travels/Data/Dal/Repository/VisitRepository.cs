using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class VisitRepository
    {
        public static Visit GetVisit(int id)
        {
            return id < Storage.Visits.Length - 1 ? Storage.Visits[id] : null;
        }

        public static bool VisitExists(int id)
        {
            return id < Storage.Visits.Length - 1 && Storage.Visits[id] != null;
        }
    }
}
