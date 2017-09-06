﻿namespace Travels.Data
{
    internal static class ValidationConstants
    {
        public const long MinVisitTimestamp = 946684800;
        public const long MaxVisitTimestamp = 1420070400;

        public const int MaxCountryLength = 50;

        public const long MinBirthdayTimestamp = -1262304000;
        public const long MaxBirthdayTimestamp = 915148800;

        public const int MaxEmailLength = 100;

        public const int MaxFirstOrLastNameLength = 50;

        public const int MaxCityLength = 50;

        public const int MinMark = 0;
        public const int MaxMark = 5;
    }
}
