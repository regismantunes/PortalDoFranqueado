using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiCollection
    {
        public static async Task<Collection[]> GetNoClosed()
            => await BaseApi.GetSimpleHttpClientRequest<Collection[]>("collections/noclosed")
                            .Get();

        public static async Task<Collection[]> GetAll()
            => await BaseApi.GetSimpleHttpClientRequest<Collection[]>("collections/all")
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

        public static async Task ChangeStatus(int id, CollectionStatus situacao)
            => await BaseApi.GetSimpleHttpClientRequest($"collections/changestatus/{id}")
                            .Put((int)situacao);

        public static async Task Update(Collection colecao)
            => await BaseApi.GetSimpleHttpClientRequest("collections")
                            .Put(colecao);
    }
}
