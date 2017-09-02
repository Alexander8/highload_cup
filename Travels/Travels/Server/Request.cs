﻿using System;
using System.Net.Sockets;
using System.Text;
using Travels.Server.Controller;

namespace Travels.Server
{
    internal sealed class Request
    {
        public readonly Socket Socket;
        public readonly byte[] Body;

        public Request(Socket socket, byte[] body)
        {
            Socket = socket;
            Body = body;
        }

        public byte[] Process(int requestSize)
        {
            try
            {
                var sb = new StringBuilder(requestSize);
                sb.Append(Encoding.UTF8.GetString(Body, 0, requestSize));

                var requestData = sb.ToString();

                if (requestData.Length < 3)
                    return PrepareResponse(Tuple.Create(400, (string)null));

                var verb = GetVerb(requestData);
                var url = requestData.Substring(verb.Length + 1, requestData.IndexOf(' ', verb.Length + 1) - verb.Length - 1).Trim('/');
                string payload = null;

                if (verb == "POST")
                {
                    const string bodySeparator = "\r\n\r\n";
                    var emptyLineIdx = requestData.LastIndexOf(bodySeparator);
                    if (emptyLineIdx == -1)
                        return PrepareResponse(Tuple.Create(400, (string)null));

                    payload = requestData.Substring(emptyLineIdx + bodySeparator.Length);

                    if (string.IsNullOrEmpty(payload))
                        return PrepareResponse(Tuple.Create(400, (string)null));
                }

                var result = Execute(verb, url, payload);
                var response = PrepareResponse(result);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process request: {ex}");
                return PrepareResponse(Tuple.Create(400, (string)null));
            }
        }

        private static string GetVerb(string data)
        {
            var verb = data.Substring(0, 3);
            if (verb != "GET")
                verb = "POST";

            return verb;
        }

        private static Tuple<int, string> Execute(string verb, string url, string payload)
        {
            Tuple<int, string> result = null;

            if (url.StartsWith("users"))
            {
                if (verb == "GET")
                {
                    if (url.Contains("/visits"))
                        result = UserController.GetVisits(url);
                    else
                        result = UserController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        result = UserController.Create(payload);
                    else
                        result = UserController.Update(url, payload);
                }
            }
            else if (url.StartsWith("locations"))
            {
                if (verb == "GET")
                {
                    if (url.Contains("/avg"))
                        result = LocationController.Avg(url);
                    else
                        result = LocationController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        result = LocationController.Create(payload);
                    else
                        result = LocationController.Update(url, payload);
                }
            }
            else if (url.StartsWith("visits"))
            {
                if (verb == "GET")
                {
                    result = VisitController.Get(url);
                }
                else
                {
                    if (url.Contains("/new"))
                        result = VisitController.Create(payload);
                    else
                        result = VisitController.Update(url, payload);
                }
            }

            return result;
        }

        private static byte[] PrepareResponse(Tuple<int, string> response)
        {
            if (response == null)
                return new byte[0];

            byte[] body = null;
            if (!string.IsNullOrEmpty(response.Item2))
                body = Encoding.UTF8.GetBytes(response.Item2);

            var statusLine = "HTTP/1.1 " + response.Item1 + " " + HttpCodeToString(response.Item1);

            var contentTypeLength = "Content-Type: application/json; charset=utf-8";
            var serverLine = "Server: Custom";
            var contentLengthLine = "Content-Length: " + (body?.Length ?? 0);

            var result = statusLine + "\r\n" + contentTypeLength + "\r\n" + serverLine + "\r\n" + contentLengthLine + "\r\n\r\n";

            var headers = Encoding.UTF8.GetBytes(result);

            var headersAndBody = new byte[headers.Length + (body?.Length ?? 0)];
            Array.Copy(headers, headersAndBody, headers.Length);

            if (body != null)
                Array.Copy(body, 0, headersAndBody, headers.Length, body.Length);

            return headersAndBody;
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
