using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiFamily
    {
        public static async Task<IEnumerable<Family>> GetFamilies(bool? withSizes)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Family>>($"family/all{(withSizes ?? false ? "/withSizes" : string.Empty)}")
                            .Get();
    }
}
