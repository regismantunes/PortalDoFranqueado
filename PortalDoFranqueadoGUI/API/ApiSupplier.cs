using PortalDoFranqueado.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiSupplier
    {
        public static async Task<Supplier[]> GetSuppliers(bool onlyActives)
            => await BaseApi.GetSimpleHttpClientRequest<Supplier[]>($"supplier/{(onlyActives ? "actives" : "all")}")
                            .Get();

        public static async Task<Supplier> Get(int id)
            => await BaseApi.GetSimpleHttpClientRequest<Supplier>($"supplier/{id}")
                            .Get();

        public static async Task<int> Insert(Supplier supplier)
            => await BaseApi.GetSimpleHttpClientRequest<int>($"supplier")
                            .Post(supplier);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"supplier/{id}")
                            .Delete();

        public static async Task Update(Supplier supplier)
            => await BaseApi.GetSimpleHttpClientRequest($"supplier")
                            .Put(supplier);
    }
}