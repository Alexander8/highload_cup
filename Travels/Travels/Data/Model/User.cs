using System;
using System.Collections.Generic;
using Travels.Data.Util;

namespace Travels.Data.Model
{
    internal sealed class User : IEquatable<User>
    {
        public readonly int Id;
        public string Email;
        public string FirstName;
        public string LastName;
        public string Gender;
        public long BirthDate;
        public int Age;

        public List<Visit> Visits;

        public User(int id, string email, string first_name, string last_name, string gender, long birth_date)
        {
            Id = id;
            Email = email;
            FirstName = first_name;
            LastName = last_name;
            Gender = gender;
            Age = ValidationUtil.TimestampToAge(birth_date);
            BirthDate = birth_date;
        }

        public override bool Equals(object obj)
        {
            var user = obj as User;
            return Equals(user);
        }

        public bool Equals(User other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
