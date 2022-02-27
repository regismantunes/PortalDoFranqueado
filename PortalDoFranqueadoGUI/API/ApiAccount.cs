using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
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

        public static async Task<User[]> GetUsers()
            => await BaseApi.GetSimpleHttpClientRequest<User[]>("account/users/all")
                            .Get();

        public static async Task<int> Insert(User user)
            => await BaseApi.GetSimpleHttpClientRequest<int>("users")
                            .Post(user);
    }
}
