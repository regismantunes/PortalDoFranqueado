using PortalDoFranqueadoAPI.Models.Enums;
using PortalDoFranqueadoAPI.Models.Validations;
using System.Collections.Generic;

namespace PortalDoFranqueadoAPI.Models.Entities
{
    [UserValidation]
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Treatment { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }
        public bool Active { get; set; }
        public IEnumerable<Store>? Stores { get; set; }
    }
}
