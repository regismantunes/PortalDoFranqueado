using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiFamily
    {
        public static async Task<Family[]> GetFamilies(bool? withSizes)
            => await BaseApi.GetSimpleHttpClientRequest<Family[]>($"family/all{(withSizes ?? false ? "/withSizes" : string.Empty)}")
                            .Get();
    }
}
