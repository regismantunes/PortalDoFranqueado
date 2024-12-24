using PortalDoFranqueadoAPI.Repositories;
using System.Threading.Tasks;
using System;
using System.Data.SqlClient;
using PortalDoFranqueadoAPI.Extensions;

namespace PortalDoFranqueadoAPI.Models.Validations
{
    public static class PurchaseSuggestionValidation
    {
        public static async Task Validate(this PurchaseSuggestion purchaseSuggestion, SqlConnection connection)
        {
            var currentCollection = await CollectionRepository.GetOpenedCollection(connection) ??
                throw new Exception("O período de compras não está aberto.");

            var purchase = await PurchaseRepository.Get(connection, purchaseSuggestion.PurchaseId) ??
                throw new Exception("O purchase Id indicado não foi enctrado no banco de dados. Entre em contato com o administrador do sistema.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new Exception("Esse período de compras não está aberto.");

            var openedPurchaseSuggestion = await PurchaseSuggestionRepository.GetByPurchaseId(connection, purchaseSuggestion.PurchaseId).AsNoContext();

            if (openedPurchaseSuggestion == null)
                return;

            purchaseSuggestion.Id = openedPurchaseSuggestion.Id;
        }
    }
}
