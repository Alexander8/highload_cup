using System;
using System.Collections.Generic;

namespace Travels.Data.Model
{
    internal sealed class Location : IEquatable<Location>
    {
        public int Id;
        public string Place;
        public string Country;
        public string City;
        public int Distance;

        public List<Visit> Visits;

        public Location(int id, string place, string country, string city, int distance)
        {
            Id = id;
            Place = place;
            Country = country;
            City = city;
            Distance = distance;
        }

        public override bool Equals(object obj)
        {
            var location = obj as Location;
            return Equals(location);
        }

        public bool Equals(Location other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
