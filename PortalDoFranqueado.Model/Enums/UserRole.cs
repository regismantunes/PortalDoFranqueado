﻿using System.ComponentModel;

namespace PortalDoFranqueado.Model.Enums
{
    public enum UserRole
    {
        [Description("Administrador")]
        Manager = 9,
        [Description("Franqueado")]
        Franchisee = 1,
    }
}
