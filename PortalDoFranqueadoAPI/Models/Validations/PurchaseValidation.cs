using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Models.Validations
{
    public static class PurchaseValidation
    {
        public static async Task Validate(this Purchase purchase, SqlConnection connection)
        {
            var currentCollection = await CollectionRepository.GetOpenedCollection(connection) ?? 
                throw new Exception("O período de compras não está aberto.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new Exception("Esse período de compras não está aberto.");

            var openedPurchase = await PurchaseRepository.Get(connection, purchase.CollectionId, purchase.StoreId, false).AsNoContext();

            if (openedPurchase == null)
                return;

            if (openedPurchase.Status != PurchaseStatus.Opened)
                throw new Exception("Essa compra não pode ser alterada porque já foi confirmada.");

            purchase.Id = openedPurchase.Id;
        }
    }
}
