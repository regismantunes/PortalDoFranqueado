using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface ISupplierRepository
    {
        Task<bool> Delete(int id);
        Task<Supplier?> Get(int id);
        Task<IEnumerable<Supplier>> GetList(bool onlyActives);
        Task<int> Insert(Supplier supplier);
        Task Update(Supplier supplier);
    }
}