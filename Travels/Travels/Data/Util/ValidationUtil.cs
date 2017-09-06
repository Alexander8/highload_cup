using System;

namespace Travels.Data.Util
{
    internal static class ValidationUtil
    {
        private static readonly double MinAge;
        private static readonly double MaxAge;

        static ValidationUtil()
        {
            MinAge = TimestampToAge(ValidationConstants.MaxBirthdayTimestamp);
            MaxAge = TimestampToAge(ValidationConstants.MinBirthdayTimestamp);
        }

        public static bool IsEmailValid(string email)
        {
            return email != null && email.Length <= ValidationConstants.MaxEmailLength;
        }

        public static bool IsFirstOrLastNameValid(string name)
        {
            return name != null && name.Length <= ValidationConstants.MaxFirstOrLastNameLength;
        }

        public static bool IsVisitDateValid(long date)
        {
            return date >= ValidationConstants.MinVisitTimestamp && date <= ValidationConstants.MaxVisitTimestamp;
        }

        public static bool IsBirthdayValid(long birth_date)
        {
            return birth_date >= ValidationConstants.MinBirthdayTimestamp && birth_date <= ValidationConstants.MaxBirthdayTimestamp;
        }

        public static bool IsAgeValid(long age)
        {
            return age >= MinAge && age <= MaxAge;
        }

        public static int TimestampToAge(long birth_date)
        {
            var ageInSeconds = DatetimeUtil.CurrentTimestamp - birth_date;
            var age = Math.Truncate(ageInSeconds / 31557600d);
            return (int)age;
        }

        public static bool IsMarkValid(long mark)
        {
            return mark >= ValidationConstants.MinMark && mark <= ValidationConstants.MaxMark;
        }

        public static bool IsGenderValid(string gender)
        {
            return gender == "m" || gender == "f";
        }

        public static bool IsPlaceValid(string place)
        {
            return place != null;
        }

        public static bool IsCityValid(string city)
        {
            return city != null && city.Length <= ValidationConstants.MaxCityLength;
        }

        public static bool IsCountryValid(string country)
        {
            return country != null && country.Length <= ValidationConstants.MaxCountryLength;
        }

        public static long GenderAsLong(string gender)
        {
            return gender == "m" ? 1 : 0;
        }

        public static string GenderAsString(long gender)
        {
            return gender == 1 ? "m" : "f";
        }
    }
}
