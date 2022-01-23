using System.Threading.Tasks;
using PortalDoFranqueadoGUI.Model;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiMainScreen
    {
        public static async Task<MainInfos> GetInfos()
            => await ExecuteGetInfos();

        public static async Task<MainInfos> GetBasicInfos()
            => await ExecuteGetInfos("basic");

        private static async Task<MainInfos> ExecuteGetInfos(string uriComplement = "")
        {
            var mainInfos = await BaseApi.GetSimpleHttpClientRequest<MainInfos>($"main/info/{uriComplement}")
                .Get();

            var repository = new Repository.FilesRepository(
                Configuration.Current.Session.User.Role != "manager",
                mainInfos.GoogleDriveClientSecret,
                mainInfos.GoogleDriveServiceCredentials,
                mainInfos.GoogleDriveApplicationName)
            {
                ApoioFolderId = mainInfos.SupportFolderId,
                FotosFolderId = mainInfos.PhotosFolderId
            };

            Configuration.Current.Session.FilesRepository = repository;

            return mainInfos;
        }

        public static async Task UpdateInformative(Informative informative)
            => await BaseApi.GetSimpleHttpClientRequest("main/informative")
                            .Put(informative);
    }
}
