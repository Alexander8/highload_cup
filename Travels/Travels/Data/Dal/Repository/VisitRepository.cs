using Travels.Data.Model;

namespace Travels.Data.Dal.Repository
{
    internal static class VisitRepository
    {
        public static Visit GetVisit(long id)
        {
            var connection = Storage.ReadConnection;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select location, user, visited_at, mark from Visit where id = @id";
                command.Parameters.AddWithValue("@id", id);

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
        }

        public static bool VisitExists(long id)
        {
            var connection = Storage.ReadConnection;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select 1 from Visit where id = @id";
                command.Parameters.AddWithValue("@id", id);

                var res = command.ExecuteScalar();
                return res != null;
            }
        }
    }
}
