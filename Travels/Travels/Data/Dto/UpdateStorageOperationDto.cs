namespace Travels.Data.Dto
{
    internal sealed class UpdateStorageOperationDto
    {
        public UpdateStorageOperationType Type { get; set; }

        public object Params { get; set; }
    }

    internal enum UpdateStorageOperationType
    {
        Unknown = 0,
        CreateUser = 1,
        UpdateUser = 2,
        CreateLocation = 3,
        UpdateLocation = 4,
        CreateVisit = 5,
        UpdateVisit = 6
    }

    internal sealed class CreateUserParamsDto
    {
        public readonly int Id;
        public readonly string Email;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Gender;
        public readonly long BirthDate;

        public CreateUserParamsDto(int id, string email, string first_name, string last_name, string gender, long birth_date)
        {
            Id = id;
            Email = email;
            FirstName = first_name;
            LastName = last_name;
            Gender = gender;
            BirthDate = birth_date;
        }
    }

    internal sealed class UpdateUserParamsDto
    {
        public readonly int Id;
        public readonly string Email;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Gender;
        public readonly long? BirthDate;

        public UpdateUserParamsDto(int id, string email, string first_name, string last_name, string gender, long? birth_date)
        {
            Id = id;
            Email = email;
            FirstName = first_name;
            LastName = last_name;
            Gender = gender;
            BirthDate = birth_date;
        }
    }

    internal sealed class CreateLocationParamsDto
    {
        public readonly int Id;
        public readonly string Place;
        public readonly string Country;
        public readonly string City;
        public readonly int Distance;

        public CreateLocationParamsDto(int id, string place, string country, string city, int distance)
        {
            Id = id;
            Place = place;
            Country = country;
            City = city;
            Distance = distance;
        }
    }

    internal sealed class UpdateLocationParamsDto
    {
        public readonly int Id;
        public readonly string Place;
        public readonly string Country;
        public readonly string City;
        public readonly int? Distance;

        public UpdateLocationParamsDto(int id, string place, string city, string country, int? distance)
        {
            Id = id;
            Place = place;
            Country = country;
            City = city;
            Distance = distance;
        }
    }

    internal sealed class CreateVisitParamsDto
    {
        public readonly int Id;
        public readonly int LocationId;
        public readonly int UserId;
        public readonly long VisitedAt;
        public readonly int Mark;

        public CreateVisitParamsDto(int id, int locationId, int userId, long visited_at, int mark)
        {
            Id = id;
            LocationId = locationId;
            UserId = userId;
            VisitedAt = visited_at;
            Mark = mark;
        }
    }

    internal sealed class UpdateVisitParamsDto
    {
        public readonly int Id;
        public readonly int? LocationId;
        public readonly int? UserId;
        public readonly long? VisitedAt;
        public readonly int? Mark;

        public UpdateVisitParamsDto(int id, int? locationId, int? userId, long? visited_at, int? mark)
        {
            Id = id;
            LocationId = locationId;
            UserId = userId;
            VisitedAt = visited_at;
            Mark = mark;
        }
    }
}
