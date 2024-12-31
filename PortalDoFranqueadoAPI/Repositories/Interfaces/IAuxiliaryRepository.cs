using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IAuxiliaryRepository
    {
        Task<IEnumerable<int>> GetIdFiles(int id);
    }
}