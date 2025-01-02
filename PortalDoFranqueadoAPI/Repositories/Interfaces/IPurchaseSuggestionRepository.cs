using PortalDoFranqueadoAPI.Models.Entities;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IPurchaseSuggestionRepository
    {
        Task<PurchaseSuggestion?> GetByPurchaseId(int purchaseId);

        Task<int> Save(PurchaseSuggestion purchaseSuggestion);
    }
}