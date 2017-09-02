using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace Travels.Data.Import
{
    internal sealed class ArchiveDataSource
    {
        public TravelsData Read(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var data = new TravelsData
            {
                Users = new List<UserData>(),
                Locations = new List<LocationData>(),
                Visits = new List<VisitData>()
            };

            using (var fstream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fstream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.StartsWith(DataConstants.Users))
                    {
                        data.Users.AddRange(Read<UserData>(entry, DataConstants.Users));
                    }
                    else if (entry.Name.StartsWith(DataConstants.Locations))
                    {
                        data.Locations.AddRange(Read<LocationData>(entry, DataConstants.Locations));
                    }
                    else if (entry.Name.StartsWith(DataConstants.Visits))
                    {
                        data.Visits.AddRange(Read<VisitData>(entry, DataConstants.Visits));
                    }
                }
            }

            Console.WriteLine("Archive data read");

            return data;
        }

        private static List<T> Read<T>(ZipArchiveEntry entry, string collectionName)
        {
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, List<T>>>(json);
                return data[collectionName];
            }
        }
    }
}
