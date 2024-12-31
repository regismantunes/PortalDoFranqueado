using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        Task<bool> Delete(int id);
        Task<Store?> Get(int id);
        Task<IEnumerable<Store>> GetList();
        Task<IEnumerable<Store>> GetListByUser(int idUser);
        Task<int> Insert(Store store);
        Task Update(Store store);
    }
}