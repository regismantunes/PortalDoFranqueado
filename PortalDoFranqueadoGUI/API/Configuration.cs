using Microsoft.Extensions.Configuration;
using PortalDoFranqueadoGUI.Model;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace PortalDoFranqueadoGUI.API
{
    public class Configuration : INotifyPropertyChanged
    {
        public string UrlBase { get; }

        private Session? _session = null;

        public Session? Session { get => _session;
            set { _session = value; OnPropertyChanged(); }
        }

        private Configuration()
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json");

            var config = builder.Build();

            UrlBase = config.GetSection("api:url").Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public static Configuration Current { get; } = new Configuration();
    }
}
