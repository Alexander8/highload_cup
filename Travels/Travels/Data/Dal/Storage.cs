﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Travels.Data.Dto;
using Travels.Data.Import;
using Travels.Data.Model;
using Travels.Data.Util;

namespace Travels.Data.Dal
{
    internal static class Storage
    {
        public static User[] Users;
        public static Location[] Locations;
        public static Visit[] Visits;

        static Storage()
        {
        }

        public static void LoadData(TravelsData data)
        {
            Users = new User[data.Users.Count + 10000];
            Locations = new Location[data.Locations.Count + 10000];
            Visits = new Visit[data.Visits.Count + 30000];

            foreach (var u in data.Users)
                Users[u.Id] = u;

            foreach (var l in data.Locations)
                Locations[l.Id] = l;

            foreach (var v in data.Visits)
            {
                Visits[v.Id] = v;
                v.User = Users[v.UserId];
                v.Location = Locations[v.LocationId];

                Debug.Assert(v.User != null);
                Debug.Assert(v.Location != null);

                if (v.User.Visits == null)
                    v.User.Visits = new List<Visit>();

                v.User.Visits.Add(v);

                if (v.Location.Visits == null)
                    v.Location.Visits = new List<Visit>();

                v.Location.Visits.Add(v);
            }

            Console.WriteLine("Data loaded to storage");
        }

        public static void CreateUser(int id, string email, string first_name, string last_name, string gender, long birth_date)
        {
            Users[id] = new User(id, email, first_name, last_name, gender, birth_date);
        }

        public static void UpdateUser(int id, string email, string first_name, string last_name, string gender, long? birth_date)
        {
            var user = Users[id];
            lock (user)
            {
                if (email != null)
                    user.Email = email;

                if (first_name != null)
                    user.FirstName = first_name;

                if (last_name != null)
                    user.LastName = last_name;

                if (gender != null)
                    user.Gender = gender;

                if (birth_date.HasValue)
                {
                    user.BirthDate = birth_date.Value;
                    user.Age = ValidationUtil.TimestampToAge(birth_date.Value);
                }
            }
        }

        public static void CreateLocation(int id, string place, string country, string city, int distance)
        {
            Locations[id] = new Location(id, place, country, city, distance);
        }

        public static void UpdateLocation(int id, string place, string city, string country, int? distance)
        {
            var location = Locations[id];
            lock (location)
            {
                if (place != null)
                    location.Place = place;

                if (city != null)
                    location.City = city;

                if (country != null)
                    location.Country = country;

                if (distance.HasValue)
                    location.Distance = distance.Value;
            }
        }

        public static void CreateVisit(int id, int locationId, int userId, long visited_at, int mark)
        {
            Visits[id] = new Visit(id, locationId, userId, visited_at, mark);

            var user = Users[userId];
            Visits[id].User = user;

            lock (user)
            {
                if (user.Visits == null)
                    user.Visits = new List<Visit>();

                user.Visits.Add(Visits[id]);
            }

            var location = Locations[locationId];
            Visits[id].Location = location;

            lock (location)
            {
                if (location.Visits == null)
                    location.Visits = new List<Visit>();

                location.Visits.Add(Visits[id]);
            }
        }

        public static void UpdateVisit(int id, int? locationId, int? userId, long? visited_at, int? mark)
        {
            var visit = Visits[id];
            lock (visit)
            {
                if (locationId.HasValue)
                {
                    visit.LocationId = locationId.Value;

                    var oldLocation = visit.Location;

                    lock (oldLocation)
                        oldLocation.Visits.Remove(visit);

                    visit.Location = Locations[locationId.Value];

                    lock (visit.Location)
                    {
                        if (visit.Location.Visits == null)
                            visit.Location.Visits = new List<Visit>();

                        visit.Location.Visits.Add(visit);
                    }
                }

                if (userId.HasValue)
                {
                    visit.UserId = userId.Value;

                    var oldUser = visit.User;

                    lock (oldUser)
                        oldUser.Visits.Remove(visit);

                    visit.User = Users[userId.Value];

                    lock (visit.User)
                    {
                        if (visit.User.Visits == null)
                            visit.User.Visits = new List<Visit>();

                        visit.User.Visits.Add(visit);
                    }
                }

                if (visited_at.HasValue)
                    visit.VisitedAt = visited_at.Value;

                if (mark.HasValue)
                    visit.Mark = mark.Value;
            }
        }
    }
}
