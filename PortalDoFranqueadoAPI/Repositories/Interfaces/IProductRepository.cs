using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> Delete(int id);

        Task<IEnumerable<Product>> GetList(int collectionId, int? familyId = null);

        Task<int> Insert(int collectionId, Product product);

        Task Update(Product product);
    }
}