using System.ComponentModel;

namespace PortalDoFranqueadoAPI.Models.Enums
{
    public enum UserRole
    {
        [Description("Administrador")]
        Manager = 9,
        [Description("Franqueado")]
        Franchisee = 1,
    }
}
