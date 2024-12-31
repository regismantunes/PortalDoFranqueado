using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IFamilyRepository
    {
        Task<IEnumerable<Family>> GetList(bool loadSizes);
    }
}