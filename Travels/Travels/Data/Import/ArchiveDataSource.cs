using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Travels.Data.Model;

namespace Travels.Data.Import
{
    internal sealed class ArchiveDataSource
    {
        public TravelsData Read(string archivePath, string optionsPath)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException(archivePath);

            if (!File.Exists(optionsPath))
                throw new FileNotFoundException(optionsPath);

            var data = new TravelsData
            {
                CurrentTimestamp = ReadTimestamp(optionsPath)
            };

            using (var fstream = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            using (var archive = new ZipArchive(fstream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.StartsWith(DataConstants.Users))
                    {
                        data.Users.AddRange(Read<User>(entry, DataConstants.Users));
                    }
                    else if (entry.Name.StartsWith(DataConstants.Locations))
                    {
                        data.Locations.AddRange(Read<Location>(entry, DataConstants.Locations));
                    }
                    else if (entry.Name.StartsWith(DataConstants.Visits))
                    {
                        data.Visits.AddRange(Read<Visit>(entry, DataConstants.Visits));
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

        private static long ReadTimestamp(string optionsPath)
        {
            var lines = File.ReadAllLines(optionsPath);
            return long.Parse(lines[0]);
        }
    }
}
