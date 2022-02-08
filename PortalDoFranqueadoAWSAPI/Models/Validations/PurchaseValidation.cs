﻿using MySqlConnector;
using PortalDoFranqueadoAPIAWS.Repositories;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPIAWS.Models.Validations
{
    public static class PurchaseValidation
    {
        public static async Task Validate(this Purchase purchase, MySqlConnection connection)
        {
            var currentCollection = await CollectionRepository.GetOpenedCollection(connection);

            if (currentCollection == null)
                throw new Exception("O período de compras não está aberto.");

            if (currentCollection.Id != purchase.CollectionId)
                throw new Exception("Esse período de compras não está aberto.");

            var openedPurchase = await PurchaseRepository.Get(connection, purchase.CollectionId, purchase.StoreId, false);

            if (openedPurchase == null)
                return;

            if (openedPurchase.Status != PurchaseStatus.Opened)
                throw new Exception("Essa compra não pode ser alterada porque já foi confirmada.");

            purchase.Id = openedPurchase.Id;
        }
    }
}
