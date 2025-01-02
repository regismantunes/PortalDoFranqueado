using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models.Entities;
using PortalDoFranqueadoAPI.Models.Enums;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Services
{
    public class PurchaseService(ICollectionRepository collectionRepository, IPurchaseRepository purchaseRepository) : IPurchaseService
    {
        public async Task Validate(Purchase purchase)
        {
            var currentCollection = await collectionRepository.GetOpenedCollection() ??
                throw new ValidationException("O período de compras não está aberto.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new ValidationException("Esse período de compras não está aberto.");

            var openedPurchase = await purchaseRepository.Get(purchase.CollectionId, purchase.StoreId, false).AsNoContext();

            if (openedPurchase == null)
                return;

            if (openedPurchase.Status != PurchaseStatus.Opened)
                throw new ValidationException("Essa compra não pode ser alterada porque já foi confirmada.");

            purchase.Id = openedPurchase.Id;
        }
    }
}
