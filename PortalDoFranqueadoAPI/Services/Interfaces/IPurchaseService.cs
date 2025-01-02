using PortalDoFranqueadoAPI.Models;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Services.Interfaces
{
    public interface IPurchaseService
    {
        Task Validate(Purchase purchase);
    }
}