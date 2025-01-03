﻿using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Enums;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class PurchaseRepository(SqlConnection connection) : IPurchaseRepository
    {
        public async Task<int> Save(Purchase purchase)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                bool newPurchase = purchase.Id == null;

                using var transaction = await connection.BeginTransactionAsync().AsNoContext();

                try
                {
                    var command = newPurchase ?
                                """
                                INSERT INTO Purchase (StoreId, CollectionId, Status)
                                OUTPUT INSERTED.Id
                                VALUES (@StoreId, @CollectionId, @Status);
                                """ :
                                """
                                UPDATE Purchase
                                    SET StoreId = @StoreId
                                    ,   CollectionId = @CollectionId
                                    ,   Status = @Status
                                    WHERE Id = @Id;
                                """;

                    using var cmd = new SqlCommand(command, connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@StoreId", purchase.StoreId);
                    cmd.Parameters.AddWithValue("@CollectionId", purchase.CollectionId);
                    cmd.Parameters.AddWithValue("@Status", (int)purchase.Status);
                    if (newPurchase)
                    {
                        var dbid = (int?)await cmd.ExecuteScalarAsync().AsNoContext()
                            ?? throw new Exception(MessageRepositories.InsertFailException);

                        purchase.Id = Convert.ToInt32(dbid);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id", purchase.Id);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.UpdateFailException);

                        cmd.Parameters.Clear();
                        cmd.CommandText = """
                                            DELETE FROM Purchase_Product
                                            WHERE PurchaseId = @PurchaseId;
                                            """;
                        cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);

                        await cmd.ExecuteNonQueryAsync().AsNoContext();
                    }

                    cmd.CommandText = """
                                        INSERT INTO Purchase_Product (PurchaseId, Item, ProductId, SizeId, Quantity)
                                        VALUES (@PurchaseId, @Item, @ProductId, @SizeId, @Quantity);
                                        """;

                    var count = 0;
                    foreach (var item in purchase.Items.Where(i => i.Quantity > 0))
                    {
                        count++;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);
                        cmd.Parameters.AddWithValue("@Item", count);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@SizeId", item.Size.Size);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    await transaction.CommitAsync().AsNoContext();

                    return purchase.Id.Value;
                }
                catch
                {
                    await transaction.RollbackAsync().AsNoContext();
                    throw;
                }
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<Purchase?> Get(int purchaseId)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT *
                                            FROM Purchase
                                            WHERE Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@Id", purchaseId);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
                {
                    var purchase = LoadPurchase(reader);
                    await reader.CloseAsync().AsNoContext();
                    await LoadPurchaseItems(purchase).AsNoContext();
                    return purchase;
                }

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        private static Purchase LoadPurchase(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                CollectionId = reader.GetInt32("CollectionId"),
                StoreId = reader.GetInt32("StoreId"),
                Status = (PurchaseStatus)reader.GetInt16("Status")
            };

        private async Task LoadPurchaseItems(Purchase purchase)
        {
            var listItems = new List<PurchaseItem>();
            var connectionWasClosed = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().AsNoContext();
                    if (connection.State != ConnectionState.Open)
                        throw new Exception(MessageRepositories.ConnectionNotOpenException);
                    connectionWasClosed = true;
                }

                using var cmd = new SqlCommand("""
                                                SELECT pp.*
                                                    ,   fs.[Order]
                                                FROM Purchase_Product AS pp
                                                    INNER JOIN Product AS p
                                                        ON p.Id = pp.ProductId
                                                    INNER JOIN Family_Size AS fs
                                                        ON fs.FamilyId = p.FamilyId
                                                        AND fs.SizeId = pp.SizeId
                                                WHERE PurchaseId = @PurchaseId;
                                                """, connection);

                cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);
                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();
                while (await reader.ReadAsync().AsNoContext())
                {
                    var productSize = new ProductSize()
                    {
                        Size = reader.GetString("SizeId"),
                        Order = reader.GetInt16("Order")
                    };

                    listItems.Add(new PurchaseItem()
                    {
                        ProductId = reader.GetInt32("ProductId"),
                        Size = productSize,
                        Quantity = reader.GetInt32("Quantity")
                    });
                }

                await reader.CloseAsync().AsNoContext();

                purchase.Items = listItems.ToArray();
            }
            finally
            {
                if (connectionWasClosed)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<Purchase?> Get(int collectionId, int storeId, bool loadItems = true)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT *
                                            FROM Purchase
                                            WHERE   CollectionId = @CollectionId
                                                AND StoreId = @StoreId;
                                            """, connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                cmd.Parameters.AddWithValue("@StoreId", storeId);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
                {
                    var purchase = LoadPurchase(reader);
                    await reader.CloseAsync().AsNoContext();
                    if (loadItems)
                        await LoadPurchaseItems(purchase).AsNoContext();

                    return purchase;
                }

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<Purchase>> GetList(int collectionId)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT *
                                            FROM Purchase
                                            WHERE CollectionId = @CollectionId;
                                            """, connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();
                var list = new List<Purchase>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(LoadPurchase(reader));

                await reader.CloseAsync().AsNoContext();

                foreach (var purchase in list)
                    await LoadPurchaseItems(purchase).AsNoContext();

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<bool> HasOpened(int collectionId)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT Id
                                            FROM Purchase
                                            WHERE CollectionId = @CollectionId
                                                AND Status = 0;
                                            """, connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);

                return await cmd.ExecuteScalarAsync().AsNoContext() != null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task Reverse(int purchaseId)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                UPDATE Purchase
                                                    SET Status = 0
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@Id", purchaseId);

                if (await cmd.ExecuteNonQueryAsync().AsNoContext() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}