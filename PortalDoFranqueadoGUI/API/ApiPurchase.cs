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
            => await BaseApi.GetSimpleHttpClientRequest<Purchase?>($"purchase/collection/{collectionId}/{storeId}")
                            .Get();

        public static async Task<Purchase?> Get(int purchaseId)
            => await BaseApi.GetSimpleHttpClientRequest<Purchase?>($"purchase/id/{purchaseId}")
                            .Get();

        public static async Task<Purchase[]> GetPurchases(int collectionId)
            => await BaseApi.GetSimpleHttpClientRequest<Purchase[]>($"purchase/collection/{collectionId}")
                            .Get();
    }
}
