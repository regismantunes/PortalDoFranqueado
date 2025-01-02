using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiCollection
    {
        public static async Task<IEnumerable<Collection>> GetNoClosed()
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Collection>>("collections/noclosed")
                            .Get();

        public static async Task<IEnumerable<Collection>> GetAll()
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Collection>>("collections/all")
                            .Get();

        public static async Task<IEnumerable<Collection>> Get(int id)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Collection>>($"collections/{id}")
                            .Get();

        public static async Task<Collection> GetOpened()
            => await BaseApi.GetSimpleHttpClientRequest<Collection>("collections/opened")
                            .Get();

        public static async Task<int> Insert(Collection colecao)
            => await BaseApi.GetSimpleHttpClientRequest<int>("collections")
                            .Post(colecao);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"collections/{id}")
                            .Delete();

        public static async Task ChangeStatus(int id, CollectionStatus status)
            => await BaseApi.GetSimpleHttpClientRequest($"collections/changestatus/{id}")
                            .Put((int)status);

        public static async Task Update(Collection colecao)
            => await BaseApi.GetSimpleHttpClientRequest("collections")
                            .Put(colecao);
    }
}
