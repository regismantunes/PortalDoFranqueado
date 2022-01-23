using PortalDoFranqueadoGUI.Repository;
using System;

namespace PortalDoFranqueadoGUI.Model
{
    public class Session
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public User User { get; set; }
        public FilesRepository? FilesRepository { get; set; }
    }
}
