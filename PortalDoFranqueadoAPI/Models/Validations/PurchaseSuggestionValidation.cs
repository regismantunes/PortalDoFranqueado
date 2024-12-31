using System.Threading.Tasks;
using System;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Validations.Interfaces;

namespace PortalDoFranqueadoAPI.Models.Validations
{
    public class PurchaseSuggestionValidation(ICollectionRepository collectionRepository, IPurchaseRepository purchaseRepository, IPurchaseSuggestionRepository purchaseSuggestionRepository) : IPurchaseSuggestionValidation
    {
        public async Task Validate(PurchaseSuggestion purchaseSuggestion)
        {
            var currentCollection = await collectionRepository.GetOpenedCollection() ??
                throw new Exception("O período de compras não está aberto.");

            var purchase = await purchaseRepository.Get(purchaseSuggestion.PurchaseId) ??
                throw new Exception("O purchase Id indicado não foi enctrado no banco de dados. Entre em contato com o administrador do sistema.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new Exception("Esse período de compras não está aberto.");

            var openedPurchaseSuggestion = await purchaseSuggestionRepository.GetByPurchaseId(purchaseSuggestion.PurchaseId).AsNoContext();

            if (openedPurchaseSuggestion == null)
                return;

            purchaseSuggestion.Id = openedPurchaseSuggestion.Id;
        }
    }
}
