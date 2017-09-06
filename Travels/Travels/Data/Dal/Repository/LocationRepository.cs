using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Text;
using Travels.Data.Model;
using Travels.Data.Util;

namespace Travels.Data.Dal.Repository
{
    internal static class LocationRepository
    {
        private const int CommandsCacheSize = 50000;

        private static readonly ConcurrentBag<SqliteCommand> GetFlatLocationCommands = new ConcurrentBag<SqliteCommand>();
        private static readonly ConcurrentBag<SqliteCommand> LocationExistsCommands = new ConcurrentBag<SqliteCommand>();

        public static void Init()
        {
            for (var i = 0; i < CommandsCacheSize; ++i)
                GetFlatLocationCommands.Add(CreateGetFlatLocationCommand());

            for (var i = 0; i < CommandsCacheSize; ++i)
                LocationExistsCommands.Add(CreateLocationExistsCommand());
        }

        public static Location GetFlatLocation(long id)
        {
            if (!GetFlatLocationCommands.TryTake(out var command))
                command = CreateGetFlatLocationCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Location
                        {
                            id = id,
                            place = (string)reader["place"],
                            country = (string)reader["country"],
                            city = (string)reader["city"],
                            distance = (long)reader["distance"],
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            finally
            {
                GetFlatLocationCommands.Add(command);
            }
        }

        public static bool LocationExists(long id)
        {
            if (!LocationExistsCommands.TryTake(out var command))
                command = CreateLocationExistsCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                var res = command.ExecuteScalar();
                return res != null;
            }
            finally
            {
                LocationExistsCommands.Add(command);
            }
        }

        public static double GetAverageLocationMark(long id, long? fromDate, long? toDate, int? fromAge, int? toAge, string gender)
        {
            var connection = Storage.ReadConnection;
            using (var command = connection.CreateCommand())
            {
                var joinPart = string.Empty;
                var wherePart = new StringBuilder();

                if (fromAge.HasValue || toAge.HasValue || gender != null)
                {
                    joinPart = "inner join User u on u.id = v.user ";

                    if (fromAge.HasValue)
                    {
                        wherePart.Append(" and u.age >= @fromAge");
                        command.Parameters.AddWithValue("@fromAge", fromAge.Value);
                    }

                    if (toAge.HasValue)
                    {
                        wherePart.Append(" and u.age < @toAge");
                        command.Parameters.AddWithValue("@toAge", toAge.Value);
                    }

                    if (gender != null)
                    {
                        wherePart.Append(" and u.gender = @gender");
                        command.Parameters.AddWithValue("@gender", ValidationUtil.GenderAsLong(gender));
                    }
                }

                if (fromDate.HasValue)
                {
                    wherePart.Append(" and v.visited_at > @fromDate");
                    command.Parameters.AddWithValue("@fromDate", fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    wherePart.Append(" and v.visited_at < @toDate");
                    command.Parameters.AddWithValue("@toDate", toDate.Value);
                }

                var commandText =
                    "select avg(v.mark) from Visit as v " +
                    (joinPart.Length > 0 ? joinPart : string.Empty) +
                    "where v.location = @location" + (wherePart.Length > 0 ? wherePart.ToString() : string.Empty);

                command.CommandText = commandText;
                command.Parameters.AddWithValue("@location", id);

                var result = command.ExecuteScalar();
                if (result == DBNull.Value)
                    return 0d;

                return Math.Round((double)result, 5);
            }
        }

        private static SqliteCommand CreateGetFlatLocationCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select place, country, city, distance from Location where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }

        private static SqliteCommand CreateLocationExistsCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select 1 from Location where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }
    }
}
