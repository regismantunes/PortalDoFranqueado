using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Models.Validations.Interfaces
{
    public interface IPurchaseValidation
    {
        Task Validate(Purchase purchase);
    }
}