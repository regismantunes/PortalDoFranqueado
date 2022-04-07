using System.Threading.Tasks;
using PortalDoFranqueado.Model;

namespace PortalDoFranqueado.API
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

            Configuration.Current.Session.AuxiliarySupportId = mainInfos.AuxiliarySupportId;
            Configuration.Current.Session.AuxiliaryPhotoId = mainInfos.AuxiliaryPhotoId;

            return mainInfos;
        }

        public static async Task UpdateInformative(Informative informative)
            => await BaseApi.GetSimpleHttpClientRequest("main/informative")
                            .Put(informative);

        public static async Task<bool> VerifyCompatibleVersion(string version)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"main/iscompatibleversion/{version}")
                            .Get();
    }
}