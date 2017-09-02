﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Travels.Data.Dal.Repository;
using Travels.Data.Dal.Service;
using Travels.Data.Dto;
using Travels.Data.Util;
using Travels.Server.Controller.Util;

namespace Travels.Server.Controller
{
    public sealed class LocationController
    {
        private const string EmptyObject = "{}";

        public static Tuple<int, string> Get(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return Tuple.Create(404, (string)null);

            var location = LocationRepository.GetFlatLocation(id);
            if (location == null)
                return Tuple.Create(404, (string)null);

            return Tuple.Create(200, JsonConvert.SerializeObject(location));
        }

        public static Tuple<int, string> Avg(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return Tuple.Create(404, (string)null);

            var queryString = ParseUtil.ParseQueryString(url);

            var fromDate = long.MinValue;
            if (queryString.ContainsKey("fromDate") && !long.TryParse(queryString["fromDate"], out fromDate))
                return Tuple.Create(400, (string)null);

            var toDate = long.MinValue;
            if (queryString.ContainsKey("toDate") && !long.TryParse(queryString["toDate"], out toDate))
                return Tuple.Create(400, (string)null);

            var fromAge = int.MinValue;
            if (queryString.ContainsKey("fromAge") && !int.TryParse(queryString["fromAge"], out fromAge))
                return Tuple.Create(400, (string)null);

            var toAge = int.MinValue;
            if (queryString.ContainsKey("toAge") && !int.TryParse(queryString["toAge"], out toAge))
                return Tuple.Create(400, (string)null);

            if (queryString.ContainsKey("gender") && !ValidationUtil.IsGenderValid(queryString["gender"]))
                return Tuple.Create(400, (string)null);

            var locationExists = LocationRepository.LocationExists(id);
            if (!locationExists)
                return Tuple.Create(404, (string)null);

            var averageMark = LocationRepository.GetAverageLocationMark(
                id,
                fromDate == long.MinValue ? (long?)null : fromDate,
                toDate == long.MinValue ? (long?)null : toDate,
                fromAge == int.MinValue ? (int?)null : fromAge,
                toAge == int.MinValue ? (int?)null : toAge,
                queryString.ContainsKey("gender") ? queryString["gender"] : null);

            return Tuple.Create(200, JsonConvert.SerializeObject(new { avg = averageMark }));
        }

        public static Tuple<int, string> Create(string payload)
        {
            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "id", int.TryParse, out var id))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "place", out var place))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "city", out var city))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "country", out var country))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "distance", int.TryParse, out var distance))
                return Tuple.Create(400, (string)null);

            if (!IsLocationValid(id, place, city, country, distance))
                return Tuple.Create(400, (string)null);

            // ReSharper disable PossibleInvalidOperationException
            UpdateStorageService.EnqueueCreateLocation(new CreateLocationParamsDto(id.Value, place, country, city, distance.Value));
            // ReSharper restore PossibleInvalidOperationException

            return Tuple.Create(200, EmptyObject);
        }

        public static Tuple<int, string> Update(string url, string payload)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return Tuple.Create(404, (string)null);

            var locationExists = LocationRepository.LocationExists(id);
            if (!locationExists)
                return Tuple.Create(404, (string)null);

            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "place", out var place))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "city", out var city))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetStringValueFromPayload(jPayload, "country", out var country))
                return Tuple.Create(400, (string)null);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "distance", int.TryParse, out var distance))
                return Tuple.Create(400, (string)null);

            if (!IsLocationToUpdateValid(place, city, country, distance))
                return Tuple.Create(400, (string)null);

            UpdateStorageService.EnqueueUpdateLocation(new UpdateLocationParamsDto(id, place, city, country, distance));

            return Tuple.Create(200, EmptyObject);
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
