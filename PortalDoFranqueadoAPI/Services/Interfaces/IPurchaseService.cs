using PortalDoFranqueadoAPI.Models.Entities;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Services.Interfaces
{
    public interface IPurchaseService
    {
        Task Validate(Purchase purchase);
    }
}