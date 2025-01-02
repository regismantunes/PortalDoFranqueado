using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Services
{
    public class PurchaseSuggestionService(ICollectionRepository collectionRepository, IPurchaseRepository purchaseRepository, IPurchaseSuggestionRepository purchaseSuggestionRepository) : IPurchaseSuggestionService
    {
        public async Task Validate(PurchaseSuggestion purchaseSuggestion)
        {
            var currentCollection = await collectionRepository.GetOpenedCollection() ??
                throw new ValidationException("O período de compras não está aberto.");

            var purchase = await purchaseRepository.Get(purchaseSuggestion.PurchaseId) ??
                throw new ValidationException("O purchase Id indicado não foi enctrado no banco de dados. Entre em contato com o administrador do sistema.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new ValidationException("Esse período de compras não está aberto.");

            var openedPurchaseSuggestion = await purchaseSuggestionRepository.GetByPurchaseId(purchaseSuggestion.PurchaseId).AsNoContext();

            if (openedPurchaseSuggestion == null)
                return;

            purchaseSuggestion.Id = openedPurchaseSuggestion.Id;
        }
    }
}
