using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Repository
{
    public class PersistentLocalRepository
    {
        private readonly string _fileAddress;
        private readonly List<string> _recentUserNames;

        public string LastUserName
        {
            get => _recentUserNames.Count > 0 ? _recentUserNames[0] : string.Empty;
            set
            {
                var savedUserName = value.ToLower();
                if (_recentUserNames.Count > 0)
                    _recentUserNames.Remove(savedUserName);

                _recentUserNames.Insert(0, savedUserName);
            }
        }

        public PersistentLocalRepository()
        {
            _recentUserNames = new List<string>();

            _fileAddress = Path.Combine(MyTempDirectory, "persist.json");
            if (File.Exists(_fileAddress))
            {
                string fileText;
                using (var stream = new StreamReader(_fileAddress))
                    fileText = stream.ReadToEnd();
                
                var persistFile = JsonSerializer.Deserialize<PersistentLocalRepositoryFile>(fileText, new JsonSerializerOptions(JsonSerializerDefaults.General));
                if (persistFile?.RecentUserNames != null)
                    _recentUserNames.AddRange(persistFile.RecentUserNames);
            }
        }

        public async Task SaveAsync()
        {
            var jsonOpitions = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            var jsonText = JsonSerializer.Serialize(new PersistentLocalRepositoryFile
            {
                RecentUserNames = _recentUserNames.ToArray()
            }, jsonOpitions);

            using var stream = new StreamWriter(_fileAddress);
            await stream.WriteAsync(jsonText);
        }

        public static string MyTempDirectory { get; } = Path.Combine(Path.GetTempPath(), "BROTHERS", "Franqueados");
    }
}
