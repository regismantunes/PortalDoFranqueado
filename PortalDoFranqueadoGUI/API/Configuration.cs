using Microsoft.Extensions.Configuration;
using PortalDoFranqueadoGUI.Model;
using System;
using System.IO;

namespace PortalDoFranqueadoGUI.API
{
    public class Configuration
    {
        public string UrlBase { get; }
        public Session? Session { get; private set; }

        private Configuration()
        {
            var builder = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json");

            var config = builder.Build();

            UrlBase = config.GetSection("api:url").Value;
        }

        public void DisconectSession()
        {
            if (Session == null)
                return;

            Session = null;
            SessionDisconected?.Invoke(this, EventArgs.Empty);
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetConectedSession(Session session)
        {
            if (Session != null)
                throw new Exception("Já existe uma sessão conectada.");

            Session = session;
            SessionConnected?.Invoke(this, EventArgs.Empty);
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SessionDisconected;
        public event EventHandler SessionConnected;
        public event EventHandler SessionChanged;

        public static Configuration Current { get; } = new Configuration();
    }
}
