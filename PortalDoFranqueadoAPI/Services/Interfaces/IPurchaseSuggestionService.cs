using PortalDoFranqueadoAPI.Models.Entities;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Services.Interfaces
{
    public interface IPurchaseSuggestionService
    {
        Task Validate(PurchaseSuggestion purchaseSuggestion);
    }
}