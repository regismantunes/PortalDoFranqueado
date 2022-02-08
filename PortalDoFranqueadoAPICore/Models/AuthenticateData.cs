using System;

namespace PortalDoFranqueadoAPICore.Models
{
    public class AuthenticateData
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
