using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiPurchase
    {
        public static async Task Save(Purchase purchas)
            => await BaseApi.GetSimpleHttpClientRequest("purchase")
                            .Put(purchas);

        public static async Task<Purchase?> Get(int collectionId, int storeId)
            => await BaseApi.GetSimpleHttpClientRequest<Purchase?>($"purchase/{collectionId}/{storeId}")
                            .Get();
    }
}
