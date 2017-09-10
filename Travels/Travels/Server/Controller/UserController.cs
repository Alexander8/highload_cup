using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Travels.Data.Dal.Repository;
using Travels.Data.Util;
using Travels.Server.Controller.Util;
using Travels.Data.Dal;

namespace Travels.Server.Controller
{
    internal static class UserController
    {
        private const string EmptyObject = "{}";

        public static ValueTuple<int, string> Get(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return ValueTuple.Create(404, (string)null);

            var user = UserRepository.GetUser(id);
            if (user == null)
                return ValueTuple.Create(404, (string)null);

            var result = new JObject
            {
                ["id"] = user.Id,
                ["email"] = user.Email,
                ["first_name"] = user.FirstName,
                ["last_name"] = user.LastName,
                ["gender"] = user.Gender,
                ["birth_date"] = user.BirthDate
            };

            return ValueTuple.Create(200, result.ToString());
        }

        public static ValueTuple<int, string> GetVisits(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return ValueTuple.Create(404, (string)null);

            var queryString = ParseUtil.ParseQueryString(url);

            var fromDate = long.MinValue;
            if (queryString.ContainsKey("fromDate") && !long.TryParse(queryString["fromDate"], out fromDate))
                return ValueTuple.Create(400, (string)null);

            var toDate = long.MinValue;
            if (queryString.ContainsKey("toDate") && !long.TryParse(queryString["toDate"], out toDate))
                return ValueTuple.Create(400, (string)null);

            if (queryString.ContainsKey("country") && string.IsNullOrEmpty(queryString["country"]))
                return ValueTuple.Create(400, (string)null);

            var toDistance = int.MinValue;
            if (queryString.ContainsKey("toDistance") && !int.TryParse(queryString["toDistance"], out toDistance))
                return ValueTuple.Create(400, (string)null);

            var userExists = UserRepository.UserExists(id);
            if (!userExists)
                return ValueTuple.Create(404, (string)null);

            var userVisits = UserRepository.GetUserVisits(
                id, 
                fromDate == long.MinValue ? (long?)null : fromDate,
                toDate == long.MinValue ? (long?)null : toDate,
                queryString.ContainsKey("country") ? Uri.UnescapeDataString(queryString["country"]).Replace('+', ' ') : null,
                toDistance == int.MinValue ? (int?)null : toDistance);

            var result = new
            {
                visits = userVisits
            };

            return ValueTuple.Create(200, JsonConvert.SerializeObject(result));
        }

        public static ValueTuple<int, string> Create(string payload)
        {
            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "id", int.TryParse, out var id))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "email", out var email))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "first_name", out var first_name))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "last_name", out var last_name))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "gender", out var gender))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "birth_date", long.TryParse, out var birth_date))
                return ValueTuple.Create(400, (string)null);

            if (!IsUserValid(id, email, first_name, last_name, gender, birth_date))
                return ValueTuple.Create(400, (string)null);

            // ReSharper disable PossibleInvalidOperationException
            Storage.CreateUser(id.Value, email, first_name, last_name, gender, birth_date.Value);
            // ReSharper restore PossibleInvalidOperationException

            return ValueTuple.Create(200, EmptyObject);
        }

        public static ValueTuple<int, string> Update(string url, string payload)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return ValueTuple.Create(404, (string)null);

            var userExists = UserRepository.UserExists(id);
            if (!userExists)
                return ValueTuple.Create(404, (string)null);

            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "email", out var email))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "first_name", out var first_name))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "last_name", out var last_name))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "gender", out var gender))
                return ValueTuple.Create(400, (string)null);

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "birth_date", long.TryParse, out var birth_date))
                return ValueTuple.Create(400, (string)null);

            if (!IsUserToUpdateValid(email, first_name, last_name, gender, birth_date))
                return ValueTuple.Create(400, (string)null);

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
    }
}
