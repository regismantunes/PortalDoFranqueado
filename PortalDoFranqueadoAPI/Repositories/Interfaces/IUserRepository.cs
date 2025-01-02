using PortalDoFranqueadoAPI.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task ChangePassword(int id, string newPassword, string newPasswordConfirmation, string? currentPassword = null);
        Task<bool> Delete(int id);
        Task<(User?, bool)> GetAuthenticated(string username, string password, short resetPasswordMaxAttempts);
        Task<IEnumerable<User>> GetList();
        Task<int> Insert(User user);
        Task<bool> ResetPassword(int id, string resetCode);
        Task Update(User user);
    }
}