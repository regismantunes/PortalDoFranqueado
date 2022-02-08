using System;

namespace PortalDoFranqueadoAPIAWS.Models
{
    public class AuthenticateData
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
