﻿using System;
using Newtonsoft.Json.Linq;
using Travels.Data.Dal.Repository;
using Travels.Data.Util;
using Travels.Server.Controller.Util;
using Travels.Data.Dal;

namespace Travels.Server.Controller
{
    public static class VisitController
    {
        private const string EmptyObject = "{}";
        private static readonly ValueTuple<int, string> BadRequest = ValueTuple.Create(400, (string)null);
        private static readonly ValueTuple<int, string> NotFound = ValueTuple.Create(404, (string)null);
        private static readonly ValueTuple<int, string> EmptyOk = ValueTuple.Create(200, EmptyObject);

        public static ValueTuple<int, string> Get(string url)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var visit = VisitRepository.GetVisit(id);
            if (visit == null)
                return NotFound;

            var result = string.Concat(
                "{\"id\":", visit.Id, ", \"location\": ", visit.LocationId, ", \"user\": ", visit.UserId, 
                ", \"visited_at\": ", visit.VisitedAt, ", \"mark\": ", visit.Mark, "}");

            return ValueTuple.Create(200, result);
        }

        public static ValueTuple<int, string> Create(string payload)
        {
            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "id", int.TryParse, out var id))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "location", int.TryParse, out var location))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "user", int.TryParse, out var user))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "visited_at", long.TryParse, out var visited_at))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "mark", int.TryParse, out var mark))
                return BadRequest;

            if (!IsVisitValid(id, location, user, visited_at, mark))
                return BadRequest;

            // ReSharper disable PossibleInvalidOperationException
            Storage.CreateVisit(id.Value, location.Value, user.Value, visited_at.Value, mark.Value);
            // ReSharper restore PossibleInvalidOperationException

            return EmptyOk;
        }

        public static ValueTuple<int, string> Update(string url, string payload)
        {
            if (!ParseUtil.TryGetIdFromUrl(url, out var id))
                return NotFound;

            var visitExists = VisitRepository.VisitExists(id);
            if (!visitExists)
                return NotFound;

            var jPayload = JToken.Parse(payload);

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "location", int.TryParse, out var location))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "user", int.TryParse, out var user))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<long>(jPayload, "visited_at", long.TryParse, out var visited_at))
                return BadRequest;

            if (!ParseUtil.TryGetValueFromPayload<int>(jPayload, "mark", int.TryParse, out var mark))
                return BadRequest;

            if (!IsVisitToUpdateValid(location, user, visited_at, mark))
                return BadRequest;

            Storage.UpdateVisit(id, location, user, visited_at, mark);

            return EmptyOk;
        }

        private static bool IsVisitValid(int? id, int? location, int? user, long? visited_at, int? mark)
        {
            if (!id.HasValue)
                return false;

            if (!location.HasValue || !LocationRepository.LocationExists(location.Value))
                return false;

            if (!user.HasValue || !UserRepository.UserExists(user.Value))
                return false;

            if (!visited_at.HasValue || !ValidationUtil.IsVisitDateValid(visited_at.Value))
                return false;

            if (!mark.HasValue || !ValidationUtil.IsMarkValid(mark.Value))
                return false;

            return true;
        }

        private static bool IsVisitToUpdateValid(int? location, int? user, long? visited_at, int? mark)
        {
            if (!location.HasValue && !user.HasValue && !visited_at.HasValue && !mark.HasValue)
                return false;

            if (location.HasValue && !LocationRepository.LocationExists(location.Value))
                return false;

            if (user.HasValue && !UserRepository.UserExists(user.Value))
                return false;

            if (visited_at.HasValue && !ValidationUtil.IsVisitDateValid(visited_at.Value))
                return false;

            if (mark.HasValue && !ValidationUtil.IsMarkValid(mark.Value))
                return false;

            return true;
        }
    }
}
