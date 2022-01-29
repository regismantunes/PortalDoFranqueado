using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiStore
    {
        public static async Task<Store[]> GetStores()
            => await BaseApi.GetSimpleHttpClientRequest<Store[]>("store/all")
                            .Get();

        public static async Task<Store?> Get(int storeId)
            => await BaseApi.GetSimpleHttpClientRequest<Store?>($"store/{storeId}")
                            .Get();
    }
}
