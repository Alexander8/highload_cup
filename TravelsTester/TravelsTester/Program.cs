using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace TravelsTester
{
    internal class Program
    {
        private static readonly Random Random = new Random();
        private static int _requestsCount = 0;

        static void Main(string[] args)
        {
            var threads = new List<Thread>();

            for (var i = 0; i < 8; ++i)
                threads.Add(new Thread(SendRequests));

            for (var i = 0; i < threads.Count; ++i)
            {
                Tuple<string, string> prms = null;

                if (i % 4 == 0)
                    prms = Tuple.Create("http://localhost:60000/locations/109535", @"{""id"":109535,""place"":""Дорожка"",""country"":""Швеция"",""city"":""Кронштадт"",""distance"":23}");
                else if (i % 4 == 1)
                    prms = Tuple.Create("http://localhost:60000/locations/286515/avg?gender=m&fromDate=810345600&toAge=44", @"{""avg"":3.0}");
                else if (i % 4 == 2)
                    prms = Tuple.Create("http://localhost:60000/users/863280", @"{""id"":863280,""email"":""decutseedhetidpe@rambler.com"",""first_name"":""Антон"",""last_name"":""Лукатотев"",""gender"":""m"",""birth_date"":-735609600}");
                else if (i % 4 == 3)
                    prms = Tuple.Create("http://localhost:60000/visits/1616532", @"{""id"":1616532,""location"":666,""user"":17838,""visited_at"":1301277086,""mark"":1}");

                threads[i].Start(prms);
            }
        }

        private static void SendRequests(object state)
        {
            Thread.Sleep(Random.Next(100));

            var prms = (Tuple<string, string>)state;

            using (var webClient = new WebClient())
            {
                while (true)
                {
                    var response = webClient.DownloadString(prms.Item1);

                    var requestsCount = Interlocked.Increment(ref _requestsCount);

                    if (requestsCount % 1000 == 0)
                        Console.WriteLine($"{DateTime.Now}: {requestsCount}");

                    if (response != prms.Item2)
                        Debugger.Break();
                }
            }
        }
    }
}
