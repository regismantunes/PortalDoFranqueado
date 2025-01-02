using System;

namespace PortalDoFranqueadoAPI.Models.Entities
{
    public class AuthenticateData
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
