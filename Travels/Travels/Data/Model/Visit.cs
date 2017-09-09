using System;

namespace Travels.Data.Model
{
    internal sealed class Visit : IEquatable<Visit>
    {
        public readonly int Id;
        public int LocationId;
        public int UserId;
        public long VisitedAt;
        public int Mark;

        public User User;
        public Location Location;

        public Visit(int id, int location, int user, long visited_at, int mark)
        {
            Id = id;
            LocationId = location;
            UserId = user;
            VisitedAt = visited_at;
            Mark = mark;
        }

        public override bool Equals(object obj)
        {
            var visit = obj as Visit;
            return Equals(visit);
        }

        public bool Equals(Visit other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
