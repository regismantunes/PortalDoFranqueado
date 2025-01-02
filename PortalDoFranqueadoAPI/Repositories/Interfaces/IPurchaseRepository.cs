using PortalDoFranqueadoAPI.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<Purchase?> Get(int purchaseId);
        Task<Purchase?> Get(int collectionId, int storeId, bool loadItems = true);
        Task<IEnumerable<Purchase>> GetList(int collectionId);
        Task<bool> HasOpened(int collectionId);
        Task Reverse(int purchaseId);
        Task<int> Save(Purchase purchase);
    }
}