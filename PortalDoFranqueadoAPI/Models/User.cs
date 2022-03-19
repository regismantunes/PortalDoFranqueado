namespace PortalDoFranqueadoAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? Treatment { get; set; }
        public string? Password { get; set; }
        public UserRole Role { get; set; }
        public bool Active { get; set; }
        public Store[]? Stores { get; set; }
    }
}
