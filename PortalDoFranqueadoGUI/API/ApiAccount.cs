using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiAccount
    {
        public static async Task Login(string username, string password)
        {
            var session = await BaseApi.GetSimpleHttpClientRequest<Session>("account/login")
                .Post(new
            {
                Username = username,
                Password = password
            });

            Configuration.Current.SetConectedSession(session);
        }

        public static async Task<IEnumerable<User>> GetUsers()
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<User>>("account/users/all")
                            .Get();

        public static async Task<int> Insert(User user)
            => await BaseApi.GetSimpleHttpClientRequest<int>("account/users")
                            .Post(user);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"account/users/{id}")
                            .Delete();

        public static async Task Update(User user)
            => await BaseApi.GetSimpleHttpClientRequest("account/users")
                            .Put(user);

        public static async Task<string> ResetPassword(int id)
            => await BaseApi.GetSimpleHttpClientRequest<string>("account/users/pswreset")
                            .Put(id);
        
        public static async Task ChangePassword(string currentPassword, string newPassword, string newPasswordConfirmation)
            => await BaseApi.GetSimpleHttpClientRequest("account/users/pswchange")
                            .Put(new
                            {
                                Id = Configuration.Current.Session.User.Id,
                                CurrentPassword = currentPassword,
                                NewPassword = newPassword,
                                NewPasswordConfirmation = newPasswordConfirmation
                            });
    }
}
