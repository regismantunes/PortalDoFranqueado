using System;
using System.Linq;

namespace PortalDoFranqueadoAPI.Models.Validations
{
    public static class UserValidation
    {
        public static void Validate(this User user)
        {
            if (user.Role == UserRole.Manager &&
                user.Stores != null &&
                user.Stores.Any())
                throw new Exception("Usuários adminstradores não podem ter lojas associadas.");

            if (user.Role == UserRole.Franchisee &&
                (user.Stores == null ||
                !user.Stores.Any()))
                throw new Exception("Usuários de franqueados deve ter ao menos uma loja associada.");
        }
    }
}
