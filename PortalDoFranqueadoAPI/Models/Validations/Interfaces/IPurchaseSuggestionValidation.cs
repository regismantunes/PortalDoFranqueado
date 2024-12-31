using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Models.Validations.Interfaces
{
    public interface IPurchaseSuggestionValidation
    {
        Task Validate(PurchaseSuggestion purchaseSuggestion);
    }
}