using System.Threading.Tasks;
using System.Linq;
using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;

namespace PortalDoFranqueado.Api
{
    public static class ApiPurchase
    {
        public static async Task<int> Save(Purchase purchase)
        {
            var cleanPurchase = new Purchase
            {
                Id = purchase.Id,
                CollectionId = purchase.CollectionId,
                StoreId = purchase.StoreId,
                Status = purchase.Status,
                Items = (from i in purchase.Items
                         select new PurchaseItem()
                         {
                             ProductId = i.ProductId,
                             Quantity = i.Quantity,
                             Size = i.Size
                         }).ToArray()
            };

            return await BaseApi.GetSimpleHttpClientRequest<int>("purchase")
                            .Put(cleanPurchase);
        }

        public static async Task<Purchase?> Get(int collectionId, int storeId)
            => await BaseApi.GetSimpleHttpClientRequest<Purchase?>($"purchase/collection/{collectionId}/{storeId}")
                            .Get();

        public static async Task<Purchase?> Get(int purchaseId)
            => await BaseApi.GetSimpleHttpClientRequest<Purchase?>($"purchase/id/{purchaseId}")
                            .Get();

        public static async Task<IEnumerable<Purchase>> GetPurchases(int collectionId)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Purchase>>($"purchase/collection/{collectionId}")
                            .Get();

        public static async Task Reverse(int purchaseId)
            => await BaseApi.GetSimpleHttpClientRequest("purchase/reverse")
                            .Put(purchaseId);
    }
}
