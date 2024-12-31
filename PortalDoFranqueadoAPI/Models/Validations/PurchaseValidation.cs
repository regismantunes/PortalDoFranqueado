using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models.Enums;
using PortalDoFranqueadoAPI.Models.Validations.Interfaces;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Models.Validations
{
    public class PurchaseValidation(ICollectionRepository collectionRepository, IPurchaseRepository purchaseRepository) : IPurchaseValidation
    {
        public async Task Validate(Purchase purchase)
        {
            var currentCollection = await collectionRepository.GetOpenedCollection() ??
                throw new Exception("O período de compras não está aberto.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new Exception("Esse período de compras não está aberto.");

            var openedPurchase = await purchaseRepository.Get(purchase.CollectionId, purchase.StoreId, false).AsNoContext();

            if (openedPurchase == null)
                return;

            if (openedPurchase.Status != PurchaseStatus.Opened)
                throw new Exception("Essa compra não pode ser alterada porque já foi confirmada.");

            purchase.Id = openedPurchase.Id;
        }
    }
}
