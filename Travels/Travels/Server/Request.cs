using System;
using System.Text;
using Travels.Server.Controller;

namespace Travels.Server
{
    internal sealed class Request
    {
        public readonly byte[] Body;

        public Request(byte[] body)
        {
            Body = body;
        }

        public byte[] Process(int requestSize)
        {
            try
            {
                var requestData = Encoding.UTF8.GetString(Body, 0, requestSize);

                if (requestData.Length < 3)
                    return PrepareResponse(ValueTuple.Create(400, (string)null));

                var verb = GetVerb(requestData);
                var url = requestData.Substring(verb.Length + 1, requestData.IndexOf(' ', verb.Length + 1) - verb.Length - 1).Trim('/');
                string payload = null;

                if (verb == "POST")
                {
                    const string bodySeparator = "\r\n\r\n";
                    var emptyLineIdx = requestData.LastIndexOf(bodySeparator);
                    if (emptyLineIdx == -1)
                        return PrepareResponse(ValueTuple.Create(400, (string)null));

                    payload = requestData.Substring(emptyLineIdx + bodySeparator.Length);

                    if (string.IsNullOrEmpty(payload))
                        return PrepareResponse(ValueTuple.Create(400, (string)null));
                }

                var result = Execute(verb, url, payload);
                var response = PrepareResponse(result);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process request: {ex}");
                return PrepareResponse(ValueTuple.Create(400, (string)null));
            }
        }

        private static string GetVerb(string data)
        {
            var verb = data.Substring(0, 3);
            if (verb != "GET")
                verb = "POST";

            return verb;
        }

        private static ValueTuple<int, string> Execute(string verb, string url, string payload)
        {
            if (url.StartsWith("users"))
            {
                if (verb == "GET")
                {
                    if (url.Contains("/visits"))
                        return UserController.GetVisits(url);
                    else
                        return UserController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        return UserController.Create(payload);
                    else
                        return UserController.Update(url, payload);
                }
            }
            else if (url.StartsWith("locations"))
            {
                if (verb == "GET")
                {
                    if (url.Contains("/avg"))
                        return LocationController.Avg(url);
                    else
                        return LocationController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        return LocationController.Create(payload);
                    else
                        return LocationController.Update(url, payload);
                }
            }
            else if (url.StartsWith("visits"))
            {
                if (verb == "GET")
                {
                    return VisitController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        return VisitController.Create(payload);
                    else
                        return VisitController.Update(url, payload);
                }
            }

            return ValueTuple.Create(400, (string)null);
        }

        private static byte[] PrepareResponse(ValueTuple<int, string> response)
        {
            var bodyLength = string.IsNullOrEmpty(response.Item2) ? 0 : Encoding.UTF8.GetByteCount(response.Item2);

            var statusLine = "HTTP/1.1 " + response.Item1 + " " + HttpCodeToString(response.Item1);

            const string contentTypeLength = "Content-Type: application/json; charset=utf-8";
            var contentLengthLine = "Content-Length: " + bodyLength;
            const string serverLine = "Server: Custom";

            var header = statusLine + "\r\n" + contentTypeLength + "\r\n" + serverLine + "\r\n" + contentLengthLine + "\r\n\r\n";

            var resultArr = new byte[header.Length + bodyLength];

            var headerArr = Encoding.UTF8.GetBytes(header);

            Array.Copy(headerArr, 0, resultArr, 0, headerArr.Length);

            if (!string.IsNullOrEmpty(response.Item2))
            {
                var bodyArr = Encoding.UTF8.GetBytes(response.Item2);
                Array.Copy(bodyArr, 0, resultArr, headerArr.Length, bodyArr.Length);
            }

            return resultArr;
        }

        private static string HttpCodeToString(int code)
        {
            switch (code)
            {
                case 400: return "Bad Request";
                case 404: return "Not Found";
                case 200: return "OK";
            }

            return string.Empty;
        }
    }
}
