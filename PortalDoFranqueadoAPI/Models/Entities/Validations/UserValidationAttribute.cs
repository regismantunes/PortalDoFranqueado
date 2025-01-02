using PortalDoFranqueadoAPI.Models.Entities;
using PortalDoFranqueadoAPI.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PortalDoFranqueadoAPI.Models.Entities.Validations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UserValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is User user)
                return Validate(user);

            ErrorMessage = "Object need be User.";
            return false;
        }

        private bool Validate(User user)
        {
            if (user.Role == UserRole.Manager &&
                user.Stores != null &&
                user.Stores.Any())
                ErrorMessage = "Usuários adminstradores não podem ter lojas associadas.";
            else if (user.Role == UserRole.Franchisee &&
                (user.Stores == null ||
                !user.Stores.Any()))
                ErrorMessage = "Usuários de franqueados deve ter ao menos uma loja associada.";
            else
                ErrorMessage = string.Empty;

            return string.IsNullOrEmpty(ErrorMessage);
        }
    }
}