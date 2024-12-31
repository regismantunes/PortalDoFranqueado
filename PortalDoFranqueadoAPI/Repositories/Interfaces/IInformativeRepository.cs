using PortalDoFranqueadoAPI.Models;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IInformativeRepository
    {
        Task<Informative> Get();

        Task Save(Informative informative);
    }
}