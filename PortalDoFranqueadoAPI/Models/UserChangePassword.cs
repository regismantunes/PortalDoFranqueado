namespace PortalDoFranqueadoAPI.Models
{
    public class UserChangePassword
    {
        public int Id { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirmation { get; set; }
    }
}
