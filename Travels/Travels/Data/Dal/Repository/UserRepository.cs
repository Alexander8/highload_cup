using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Travels.Data.Dto;
using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class UserRepository
    {
        private const int CommandsCacheSize = 50000;

        private static readonly ConcurrentBag<SqliteCommand> GetFlatUserCommands = new ConcurrentBag<SqliteCommand>();
        private static readonly ConcurrentBag<SqliteCommand> UserExistsCommands = new ConcurrentBag<SqliteCommand>();

        public static void Init()
        {
            for (var i = 0; i < CommandsCacheSize; ++i)
                GetFlatUserCommands.Add(CreateGetFlatUserCommand());

            for (var i = 0; i < CommandsCacheSize; ++i)
                UserExistsCommands.Add(CreateUserExistsCommand());
        }

        public static User GetFlatUser(int id)
        {
            if (!GetFlatUserCommands.TryTake(out var command))
                command = CreateGetFlatUserCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            id = id,
                            email = (string)reader["email"],
                            first_name = (string)reader["first_name"],
                            last_name = (string)reader["last_name"],
                            gender = (long)reader["gender"],
                            birth_date = (long)reader["birth_date"]
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
                GetFlatUserCommands.Add(command);
            }
        }

        public static bool UserExists(int id)
        {
            if (!UserExistsCommands.TryTake(out var command))
                command = CreateUserExistsCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                var res = command.ExecuteScalar();
                return res != null;
            }
            finally
            {
                UserExistsCommands.Add(command);
            }
        }

        public static IEnumerable<UserVisitToLocationDto> GetUserVisits(int id, long? fromDate, long? toDate, string country, int? toDistance)
        {
            var connection = Storage.ReadConnection;
            using (var command = connection.CreateCommand())
            {
                var wherePart = new StringBuilder();

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

                if (country != null)
                {
                    wherePart.Append(" and l.country = @country");
                    command.Parameters.AddWithValue("@country", country);
                }

                if (toDistance != null)
                {
                    wherePart.Append(" and l.distance < @toDistance");
                    command.Parameters.AddWithValue("@toDistance", toDistance.Value);
                }

                var commandText =
                    "select l.place, v.mark, v.visited_at from Visit as v " +
                    "join Location l on l.id = v.location " +
                    "where v.user = @user" + (wherePart.Length > 0 ? wherePart.ToString() : string.Empty) +
                    " order by v.visited_at";

                command.CommandText = commandText;
                command.Parameters.AddWithValue("@user", id);

                var result = new List<UserVisitToLocationDto>(50);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new UserVisitToLocationDto
                        {
                            place = (string)reader["place"],
                            mark = (long)reader["mark"],
                            visited_at = (long)reader["visited_at"],
                        });
                    }
                }

                return result;
            }        
        }

        private static SqliteCommand CreateGetFlatUserCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select email, first_name, last_name, gender, birth_date from User where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }

        private static SqliteCommand CreateUserExistsCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select 1 from User where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }
    }
}
