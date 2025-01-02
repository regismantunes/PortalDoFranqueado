using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface ICollectionRepository
    {
        Task<CollectionInfo> GetInfo();

        Task<IEnumerable<Collection>> GetList(bool onlyActives = true);

        Task<Collection?> Get(int id);

        Task<bool> HasOpenedCollection();

        Task<Collection?> GetOpenedCollection();

        Task ChangeStatus(int id, CollectionStatus status);

        Task<int> Insert(Collection collection);

        Task<bool> Delete(int id);

        Task Update(Collection colecao);
    }
}