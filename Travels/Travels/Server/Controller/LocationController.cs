using System;
using Newtonsoft.Json.Linq;
using Travels.Data.Dal.Repository;
using Travels.Data.Util;
using Travels.Server.Controller.Util;
using Travels.Data.Dal;

namespace Travels.Server.Controller
{
    public sealed class LocationController
    {
        private const string EmptyObject = "{}";
        private static readonly ValueTuple<int, string> BadRequest = ValueTuple.Create(400, (string)null);
        private static readonly ValueTuple<int, string> NotFound = ValueTuple.Create(404, (string)null);

        public static ValueTuple<int, string> Get(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var location = LocationRepository.GetLocation(id);
            if (location == null)
                return NotFound;

            var result = string.Concat(
                "{\"id\":", location.Id, ", \"place\": \"", location.Place, "\", ",
                "\"country\": \"", location.Country, "\", \"city\": \"", location.City,
                "\", \"distance\": ", location.Distance, "}");

            return ValueTuple.Create(200, result);
        }

        public static ValueTuple<int, string> Avg(string url)
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

            var fromAge = int.MinValue;
            if (queryString.ContainsKey("fromAge") && !int.TryParse(queryString["fromAge"], out fromAge))
                return BadRequest;

            var toAge = int.MinValue;
            if (queryString.ContainsKey("toAge") && !int.TryParse(queryString["toAge"], out toAge))
                return BadRequest;

            if (queryString.ContainsKey("gender") && !ValidationUtil.IsGenderValid(queryString["gender"]))
                return BadRequest;

            var locationExists = LocationRepository.LocationExists(id);
            if (!locationExists)
                return NotFound;

            var averageMark = LocationRepository.GetAverageLocationMark(
                id,
                fromDate == long.MinValue ? (long?)null : fromDate,
                toDate == long.MinValue ? (long?)null : toDate,
                fromAge == int.MinValue ? (int?)null : fromAge,
                toAge == int.MinValue ? (int?)null : toAge,
                queryString.ContainsKey("gender") ? queryString["gender"] : null);

            var result = "{ \"avg\": " + averageMark + "}";

            return ValueTuple.Create(200, result);
        }

        public static ValueTuple<int, string> Create(string payload)
        {
            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "id", int.TryParse, out var id))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "place", out var place))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "city", out var city))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "country", out var country))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "distance", int.TryParse, out var distance))
                return BadRequest;

            if (!IsLocationValid(id, place, city, country, distance))
                return BadRequest;

            // ReSharper disable PossibleInvalidOperationException
            Storage.CreateLocation(id.Value, place, country, city, distance.Value);
            // ReSharper restore PossibleInvalidOperationException

            return ValueTuple.Create(200, EmptyObject);
        }

        public static ValueTuple<int, string> Update(string url, string payload)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var locationExists = LocationRepository.LocationExists(id);
            if (!locationExists)
                return NotFound;

            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "place", out var place))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "city", out var city))
                return BadRequest;

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "country", out var country))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "distance", int.TryParse, out var distance))
                return BadRequest;

            if (!IsLocationToUpdateValid(place, city, country, distance))
                return BadRequest;

            Storage.UpdateLocation(id, place, city, country, distance);

            return ValueTuple.Create(200, EmptyObject);
        }

        private static bool IsLocationValid(int? id, string place, string city, string country, int? distance)
        {
            if (!id.HasValue)
                return false;

            if (!ValidationUtil.IsPlaceValid(place))
                return false;

            if (!ValidationUtil.IsCityValid(city))
                return false;

            if (!ValidationUtil.IsCountryValid(country))
                return false;

            if (!distance.HasValue)
                return false;

            return true;
        }

        private static bool IsLocationToUpdateValid(string place, string city, string country, int? distance)
        {
            if (place == null && city == null && country == null && !distance.HasValue)
                return false;

            if (place != null && !ValidationUtil.IsPlaceValid(place))
                return false;

            if (city != null && !ValidationUtil.IsCityValid(city))
                return false;

            if (country != null && !ValidationUtil.IsCountryValid(country))
                return false;

            return true;
        }
    }
}
