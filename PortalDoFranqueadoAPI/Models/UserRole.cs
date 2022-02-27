using System.ComponentModel;

namespace PortalDoFranqueadoAPI.Models
{
    public enum UserRole
    {
        [Description("Administrador")]
        Manager = 9,
        [Description("Franqueado")]
        Franchisee = 1,
    }
}
