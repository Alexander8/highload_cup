using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Travels.Data.Dal.Repository;
using Travels.Data.Util;
using Travels.Server.Controller.Util;
using Travels.Data.Dal;
using Travels.Data.Dto;

namespace Travels.Server.Controller
{
    internal static class UserController
    {
        private const string EmptyObject = "{}";
        private static readonly ValueTuple<int, string> BadRequest = ValueTuple.Create(400, (string)null);
        private static readonly ValueTuple<int, string> NotFound = ValueTuple.Create(404, (string)null);

        public static ValueTuple<int, string> Get(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var user = UserRepository.GetUser(id);
            if (user == null)
                return NotFound;

            var result = string.Concat(
                "{\"id\":", user.Id, ", \"email\": \"", user.Email, "\", \"first_name\": \"", user.FirstName, 
                "\", \"last_name\": \"", user.LastName, "\", \"gender\": \"", user.Gender, 
                "\", \"birth_date\": ", user.BirthDate, "}");

            return ValueTuple.Create(200, result);
        }

        public static ValueTuple<int, string> GetVisits(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var queryString = ParseUtil.ParseQueryString(url);

            var fromDate = long.MinValue;
            if (queryString.ContainsKey("fromDate") && !long.TryParse(queryString["fromDate"], out fromDate))
                return BadRequest;

            var toDate = long.MinValue;
            if (queryString.ContainsKey("toDate") && !long.TryParse(queryString["toDate"], out toDate))
                return BadRequest;

            if (queryString.ContainsKey("country") && string.IsNullOrEmpty(queryString["country"]))
                return BadRequest;

            var toDistance = int.MinValue;
            if (queryString.ContainsKey("toDistance") && !int.TryParse(queryString["toDistance"], out toDistance))
                return BadRequest;

            var userExists = UserRepository.UserExists(id);
            if (!userExists)
                return NotFound;

            var userVisits = UserRepository.GetUserVisits(
                id, 
                fromDate == long.MinValue ? (long?)null : fromDate,
                toDate == long.MinValue ? (long?)null : toDate,
                queryString.ContainsKey("country") ? Uri.UnescapeDataString(queryString["country"]).Replace('+', ' ') : null,
                toDistance == int.MinValue ? (int?)null : toDistance);

            return ValueTuple.Create(200, SerializeUserVisits(userVisits));
        }

        public static ValueTuple<int, string> Create(string payload)
        {
            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "id", int.TryParse, out var id))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "email", out var email))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "first_name", out var first_name))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "last_name", out var last_name))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "gender", out var gender))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "birth_date", long.TryParse, out var birth_date))
                return BadRequest;

            if (!IsUserValid(id, email, first_name, last_name, gender, birth_date))
                return BadRequest;

            // ReSharper disable PossibleInvalidOperationException
            Storage.CreateUser(id.Value, email, first_name, last_name, gender, birth_date.Value);
            // ReSharper restore PossibleInvalidOperationException

            return ValueTuple.Create(200, EmptyObject);
        }

        public static ValueTuple<int, string> Update(string url, string payload)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var userExists = UserRepository.UserExists(id);
            if (!userExists)
                return NotFound;

            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "email", out var email))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "first_name", out var first_name))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "last_name", out var last_name))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "gender", out var gender))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "birth_date", long.TryParse, out var birth_date))
                return BadRequest;

            if (!IsUserToUpdateValid(email, first_name, last_name, gender, birth_date))
                return BadRequest;

            Storage.UpdateUser(id, email, first_name, last_name, gender, birth_date);

            return ValueTuple.Create(200, EmptyObject);
        }

        private static bool IsUserValid(int? id, string email, string first_name, string last_name, string gender, long? birth_date)
        {
            if (!id.HasValue)
                return false;

            if (!ValidationUtil.IsEmailValid(email))
                return false;

            if (!ValidationUtil.IsFirstOrLastNameValid(first_name))
                return false;

            if (!ValidationUtil.IsFirstOrLastNameValid(last_name))
                return false;

            if (!ValidationUtil.IsGenderValid(gender))
                return false;

            if (!birth_date.HasValue || !ValidationUtil.IsBirthdayValid(birth_date.Value))
                return false;

            return true;
        }

        private static bool IsUserToUpdateValid(string email, string first_name, string last_name, string gender, long? birth_date)
        {
            if (email == null && first_name == null && last_name == null && gender == null && !birth_date.HasValue)
                return false;

            if (email != null && !ValidationUtil.IsEmailValid(email))
                return false;

            if (first_name != null && !ValidationUtil.IsFirstOrLastNameValid(first_name))
                return false;

            if (last_name != null && !ValidationUtil.IsFirstOrLastNameValid(last_name))
                return false;

            if (gender != null && !ValidationUtil.IsGenderValid(gender))
                return false;

            if (birth_date.HasValue && !ValidationUtil.IsBirthdayValid(birth_date.Value))
                return false;

            return true;
        }

        private static string SerializeUserVisits(IEnumerable<UserVisitToLocationDto> userVisits)
        {
            var sb = new StringBuilder(80 * 10);

            sb.Append("{\"visits\":[");

            var initialLength = sb.Length;

            foreach (var userVisit in userVisits)
                sb.Append(string.Concat("{\"mark\":", userVisit.mark, ",\"visited_at\":", userVisit.visited_at, ",\"place\":\"", userVisit.place, "\"},"));

            if (initialLength < sb.Length)
                sb.Remove(sb.Length - 1, 1);

            sb.Append("]}");

            return sb.ToString();
        }
    }
}
