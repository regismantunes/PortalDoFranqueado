using System;

namespace PortalDoFranqueado.Model
{
    public class Session
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public User User { get; set; }
        public bool ResetPassword { get; set; }
        public int? AuxiliarySupportId { get; set; }
        public int? AuxiliaryPhotoId { get; set; }
    }
}