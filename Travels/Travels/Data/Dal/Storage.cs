using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using Travels.Data.Dto;
using Travels.Data.Import;
using Travels.Data.Util;

namespace Travels.Data.Dal
{
    internal static class Storage
    {
        private static readonly string ConnectionString;
        private static readonly DateTime Now = DateTime.Now;

        public static readonly SqliteConnection ReadConnection;
        private static readonly SqliteConnection WriteConnection;

        private static SqliteCommand _createUserCommand;
        private static SqliteCommand _createLocationCommand;
        private static SqliteCommand _createVisitCommand;

        static Storage()
        {
            var connStrBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = ":memory:",
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.Memory             
            };

            ConnectionString = connStrBuilder.ToString();
            ReadConnection = new SqliteConnection(ConnectionString);
            ReadConnection.Open();

            WriteConnection = new SqliteConnection(ConnectionString);
            WriteConnection.Open();

            InitUpdateCommands();
        }

        public static void LoadData(TravelsData data)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                CreateSchema(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    InsertUsers(data.Users, connection);
                    InsertLocations(data.Locations, connection);
                    InsertVisits(data.Visits, connection);

                    transaction.Commit();
                }

                CreateIndexes(connection);
            }

            Console.WriteLine("Data loaded to storage");
        }

        public static void CreateUser(CreateUserParamsDto createParams)
        {
            _createUserCommand.Parameters[0].Value = createParams.Id;
            _createUserCommand.Parameters[1].Value = createParams.Email;
            _createUserCommand.Parameters[2].Value = createParams.FirstName;
            _createUserCommand.Parameters[3].Value = createParams.LastName;
            _createUserCommand.Parameters[4].Value = ValidationUtil.GenderAsLong(createParams.Gender);
            _createUserCommand.Parameters[5].Value = createParams.BirthDate;
            _createUserCommand.Parameters[6].Value = ValidationUtil.TimestampToAge(createParams.BirthDate, Now);
            _createUserCommand.ExecuteNonQuery();
        }

        public static void CreateLocation(CreateLocationParamsDto createParams)
        {
            _createLocationCommand.Parameters[0].Value = createParams.Id;
            _createLocationCommand.Parameters[1].Value = createParams.Place;
            _createLocationCommand.Parameters[2].Value = createParams.Country;
            _createLocationCommand.Parameters[3].Value = createParams.City;
            _createLocationCommand.Parameters[4].Value = createParams.Distance;
            _createLocationCommand.ExecuteNonQuery();
        }

        public static void CreateVisit(CreateVisitParamsDto createParams)
        {
            _createVisitCommand.Parameters[0].Value = createParams.Id;
            _createVisitCommand.Parameters[1].Value = createParams.LocationId;
            _createVisitCommand.Parameters[2].Value = createParams.UserId;
            _createVisitCommand.Parameters[3].Value = createParams.VisitedAt;
            _createVisitCommand.Parameters[4].Value = createParams.Mark;
            _createVisitCommand.ExecuteNonQuery();
        }

        public static void UpdateUser(UpdateUserParamsDto updateParams)
        {
            using (var command = WriteConnection.CreateCommand())
            {
                var setAdded = false;
                var commandText = new StringBuilder();
                commandText.Append("update User set ");                

                if (updateParams.Email != null)
                {
                    commandText.Append("email = @email");
                    command.Parameters.AddWithValue("@email", updateParams.Email);
                    setAdded = true;
                }

                if (updateParams.FirstName != null)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("first_name = @first_name");
                    command.Parameters.AddWithValue("@first_name", updateParams.FirstName);
                    setAdded = true;
                }

                if (updateParams.LastName != null)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("last_name = @last_name");
                    command.Parameters.AddWithValue("@last_name", updateParams.LastName);
                    setAdded = true;
                }

                if (updateParams.Gender != null)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("gender = @gender");
                    command.Parameters.AddWithValue("@gender", ValidationUtil.GenderAsLong(updateParams.Gender));
                    setAdded = true;
                }

                if (updateParams.BirthDate.HasValue)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("birth_date = @birth_date,age = @age");
                    command.Parameters.AddWithValue("@birth_date", updateParams.BirthDate.Value);
                    command.Parameters.AddWithValue("@age", ValidationUtil.TimestampToAge(updateParams.BirthDate.Value, Now));

                    setAdded = true;
                }

                commandText.Append(" where id = @id");
                command.Parameters.AddWithValue("@id", updateParams.Id);

                command.CommandText = commandText.ToString();

                command.ExecuteNonQuery();
            }
        }

        public static void UpdateLocation(UpdateLocationParamsDto updateParams)
        {
            using (var command = WriteConnection.CreateCommand())
            {
                var setAdded = false;
                var commandText = new StringBuilder();
                commandText.Append("update Location set ");

                if (updateParams.Place != null)
                {
                    commandText.Append("place = @place");
                    command.Parameters.AddWithValue("@place", updateParams.Place);
                    setAdded = true;
                }

                if (updateParams.Country != null)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("country = @country");
                    command.Parameters.AddWithValue("@country", updateParams.Country);
                    setAdded = true;
                }

                if (updateParams.City != null)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("city = @city");
                    command.Parameters.AddWithValue("@city", updateParams.City);
                    setAdded = true;
                }

                if (updateParams.Distance.HasValue)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("distance = @distance");
                    command.Parameters.AddWithValue("@distance", updateParams.Distance.Value);
                    setAdded = true;
                }

                commandText.Append(" where id = @id");
                command.Parameters.AddWithValue("@id", updateParams.Id);

                command.CommandText = commandText.ToString();

                command.ExecuteNonQuery();
            }
        }

        public static void UpdateVisit(UpdateVisitParamsDto updateParams)
        {
            using (var command = WriteConnection.CreateCommand())
            {
                var setAdded = false;
                var commandText = new StringBuilder();
                commandText.Append("update Visit set ");             

                if (updateParams.LocationId.HasValue)
                {
                    commandText.Append("location = @location");
                    command.Parameters.AddWithValue("@location", updateParams.LocationId.Value);
                    setAdded = true;
                }

                if (updateParams.UserId.HasValue)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("user = @user");
                    command.Parameters.AddWithValue("@user", updateParams.UserId.Value);
                    setAdded = true;
                }

                if (updateParams.VisitedAt.HasValue)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("visited_at = @visited_at");
                    command.Parameters.AddWithValue("@visited_at", updateParams.VisitedAt.Value);
                    setAdded = true;
                }

                if (updateParams.Mark.HasValue)
                {
                    if (setAdded)
                        commandText.Append(",");

                    commandText.Append("mark = @mark");
                    command.Parameters.AddWithValue("@mark", updateParams.Mark.Value);
                    setAdded = true;
                }

                commandText.Append(" where id = @id");
                command.Parameters.AddWithValue("@id", updateParams.Id);

                command.CommandText = commandText.ToString();

                command.ExecuteNonQuery();
            }
        }

        private static void InitUpdateCommands()
        {
            _createUserCommand = WriteConnection.CreateCommand();
            _createUserCommand.CommandText = @"
                            INSERT INTO User(id, email, first_name, last_name, gender, birth_date, age) 
                            values(@id, @email, @first_name, @last_name, @gender, @birth_date, @age);";

            _createUserCommand.Parameters.AddRange(new[] {
                    new SqliteParameter("@id", null),
                    new SqliteParameter("@email", null),
                    new SqliteParameter("@first_name", null),
                    new SqliteParameter("@last_name", null),
                    new SqliteParameter("@gender", null),
                    new SqliteParameter("@birth_date", null),
                    new SqliteParameter("@age", null)
                });

            _createLocationCommand = WriteConnection.CreateCommand();
            _createLocationCommand.CommandText = @"
                            INSERT INTO Location(id, place, country, city, distance) 
                            values(@id, @place, @country, @city, @distance);";

            _createLocationCommand.Parameters.AddRange(new[] {
                    new SqliteParameter("@id", null),
                    new SqliteParameter("@place", null),
                    new SqliteParameter("@country", null),
                    new SqliteParameter("@city", null),
                    new SqliteParameter("@distance", null)
                });

            _createVisitCommand = WriteConnection.CreateCommand();
            _createVisitCommand.CommandText = @"
                            INSERT INTO Visit(id, location, user, visited_at, mark) 
                            values(@id, @location, @user, @visited_at, @mark);";

            _createVisitCommand.Parameters.AddRange(new[] {
                    new SqliteParameter("@id", null),
                    new SqliteParameter("@location", null),
                    new SqliteParameter("@user", null),
                    new SqliteParameter("@visited_at", null),
                    new SqliteParameter("@mark", null)
                });
        }

        private static void CreateSchema(SqliteConnection connection)
        {
            ExecutePragmas(connection);
            CreateTables(connection);
        }

        private static void ExecutePragmas(SqliteConnection connection)
        {
            var pragmas = new[] 
            {
                "PRAGMA journal_mode = OFF;",
                "PRAGMA synchronous = OFF;",
            };

            using (var command = connection.CreateCommand())
            {
                foreach (var pragma in pragmas)
                {
                    command.CommandText = pragma;
                    var result = command.ExecuteScalar();
                }
            }
        }

        private static void CreateTables(SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE User(
                        id INTEGER PRIMARY KEY, 
                        email TEXT NOT NULL, 
                        first_name TEXT NOT NULL, 
                        last_name TEXT NOT NULL, 
                        gender INTEGER NOT NULL, 
                        birth_date INTEGER NOT NULL,
                        age REAL NOT NULL
                    ) WITHOUT ROWID";

                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE Location(
                        id INTEGER PRIMARY KEY, 
                        place TEXT NOT NULL, 
                        country TEXT NOT NULL, 
                        city TEXT NOT NULL, 
                        distance INTEGER NOT NULL
                    ) WITHOUT ROWID";

                command.ExecuteNonQuery();

                command.CommandText = @"
                    CREATE TABLE Visit(
                        id INTEGER PRIMARY KEY, 
                        location INTEGER NOT NULL, 
                        user INTEGER NOT NULL, 
                        visited_at INTEGER NOT NULL, 
                        mark INTEGER NOT NULL
                    ) WITHOUT ROWID";

                command.ExecuteNonQuery();
            }
        }

        private static void InsertUsers(List<UserData> users, SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                            INSERT INTO User(id, email, first_name, last_name, gender, birth_date, age) 
                            values(@id, @email, @first_name, @last_name, @gender, @birth_date, @age);";

                command.Parameters.AddRange(new[] {
                        new SqliteParameter("@id", null),
                        new SqliteParameter("@email", null),
                        new SqliteParameter("@first_name", null),
                        new SqliteParameter("@last_name", null),
                        new SqliteParameter("@gender", null),
                        new SqliteParameter("@birth_date", null),
                        new SqliteParameter("@age", null)
                    });

                foreach (var user in users)
                {
                    command.Parameters[0].Value = user.id;
                    command.Parameters[1].Value = user.email;
                    command.Parameters[2].Value = user.first_name;
                    command.Parameters[3].Value = user.last_name;
                    command.Parameters[4].Value = ValidationUtil.GenderAsLong(user.gender);
                    command.Parameters[5].Value = user.birth_date;
                    command.Parameters[6].Value = ValidationUtil.TimestampToAge(user.birth_date, Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertLocations(List<LocationData> locations, SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                            INSERT INTO Location(id, place, country, city, distance) 
                            values(@id, @place, @country, @city, @distance);";

                command.Parameters.AddRange(new[] {
                        new SqliteParameter("@id", null),
                        new SqliteParameter("@place", null),
                        new SqliteParameter("@country", null),
                        new SqliteParameter("@city", null),
                        new SqliteParameter("@distance", null)
                    });

                foreach (var location in locations)
                {
                    command.Parameters[0].Value = location.id;
                    command.Parameters[1].Value = location.place;
                    command.Parameters[2].Value = location.country;
                    command.Parameters[3].Value = location.city;
                    command.Parameters[4].Value = location.distance;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void InsertVisits(List<VisitData> visits, SqliteConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                            INSERT INTO Visit(id, location, user, visited_at, mark) 
                            values(@id, @location, @user, @visited_at, @mark);";

                command.Parameters.AddRange(new[] {
                        new SqliteParameter("@id", null),
                        new SqliteParameter("@location", null),
                        new SqliteParameter("@user", null),
                        new SqliteParameter("@visited_at", null),
                        new SqliteParameter("@mark", null)
                    });

                foreach (var visit in visits)
                {
                    command.Parameters[0].Value = visit.id;
                    command.Parameters[1].Value = visit.location;
                    command.Parameters[2].Value = visit.user;
                    command.Parameters[3].Value = visit.visited_at;
                    command.Parameters[4].Value = visit.mark;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static void CreateIndexes(SqliteConnection connection)
        {
            var indexes = new[]
            {
                "CREATE INDEX IX_USER_AGE ON User(age);",

                "CREATE INDEX IX_LOCATION_COUNTRY ON Location(country);",
                "CREATE INDEX IX_LOCATION_DISTANCE ON Location(distance);",

                "CREATE INDEX IX_VISIT_USER ON Visit(user);",
                "CREATE INDEX IX_VISIT_LOCATION ON Visit(location);",
                "CREATE INDEX IX_VISIT_VISITED_AT ON Visit(visited_at);",
            };

            using (var command = connection.CreateCommand())
            {
                foreach (var index in indexes)
                {
                    command.CommandText = index;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
