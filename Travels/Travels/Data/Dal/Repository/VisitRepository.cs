using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class VisitRepository
    {
        private const int CommandsCacheSize = 50000;

        private static readonly ConcurrentBag<SqliteCommand> GetVisitCommands = new ConcurrentBag<SqliteCommand>();
        private static readonly ConcurrentBag<SqliteCommand> VisitExistsCommands = new ConcurrentBag<SqliteCommand>();

        public static void Init()
        {
            for (var i = 0; i < CommandsCacheSize; ++i)
                GetVisitCommands.Add(CreateGetVisitCommand());

            for (var i = 0; i < CommandsCacheSize; ++i)
                VisitExistsCommands.Add(CreateVisitExistsCommand());
        }

        public static Visit GetVisit(long id)
        {
            if (!GetVisitCommands.TryTake(out var command))
                command = CreateGetVisitCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Visit
                        {
                            id = id,
                            location = (long)reader["location"],
                            user = (long)reader["user"],
                            visited_at = (long)reader["visited_at"],
                            mark = (long)reader["mark"],
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
                GetVisitCommands.Add(command);
            }
        }

        public static bool VisitExists(long id)
        {
            if (!VisitExistsCommands.TryTake(out var command))
                command = CreateVisitExistsCommand();

            try
            {
                command.Parameters["@id"].Value = id;

                var res = command.ExecuteScalar();
                return res != null;
            }
            finally
            {
                VisitExistsCommands.Add(command);
            }
        }

        private static SqliteCommand CreateGetVisitCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select location, user, visited_at, mark from Visit where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }

        private static SqliteCommand CreateVisitExistsCommand()
        {
            var command = Storage.ReadConnection.CreateCommand();
            command.CommandText = "select 1 from Visit where id = @id";
            command.Parameters.AddWithValue("@id", null);
            return command;
        }
    }
}
