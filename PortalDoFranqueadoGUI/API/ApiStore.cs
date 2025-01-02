using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiStore
    {
        public static async Task<IEnumerable<Store>> GetStores()
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Store>>("store/all")
                            .Get();

        public static async Task<Store?> Get(int storeId)
            => await BaseApi.GetSimpleHttpClientRequest<Store?>($"store/{storeId}")
                            .Get();

        public static async Task<int> Insert(Store store)
            => await BaseApi.GetSimpleHttpClientRequest<int>($"store")
                            .Post(store);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"store/{id}")
                            .Delete();

        public static async Task Update(Store store)
            => await BaseApi.GetSimpleHttpClientRequest($"store")
                            .Put(store);
    }
}
