﻿using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class PurchaseRepository
    {
        public static async Task Save(SqlConnection connection, Purchase purchase)
        {
            await purchase.Validate(connection);

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                bool newPurchase = purchase.Id == null;

                var transaction = await connection.BeginTransactionAsync();

                try
                {
                    using var cmd = new SqlCommand()
                    {
                        Connection = connection,
                        CommandText = newPurchase ?
                                    "INSERT INTO Purchase (StoreId, CollectionId, Status)" +
                                        " VALUES (@StoreId, @CollectionId, @Status);" :
                                    "UPDATE Purchase" +
                                        " SET StoreId = @StoreId" +
                                            ", CollectionId = @CollectionId" +
                                            ", Status = @Status" +
                                        " WHERE Id = @Id;",
                        Transaction = (SqlTransaction)transaction
                    };

                    cmd.Parameters.AddWithValue("@StoreId", purchase.StoreId);
                    cmd.Parameters.AddWithValue("@CollectionId", purchase.CollectionId);
                    cmd.Parameters.AddWithValue("@Status", (int)purchase.Status);
                    if (!newPurchase)
                        cmd.Parameters.AddWithValue("@Id", purchase.Id);

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    if (newPurchase)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT SCOPE_IDENTITY();";

                        var dbid = (ulong)await cmd.ExecuteScalarAsync();
                        purchase.Id = Convert.ToInt32(dbid);
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM Purchase_Product" +
                                        " WHERE PurchaseId = @PurchaseId;";
                        cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    cmd.CommandText = "INSERT INTO Purchase_Product (PurchaseId, Item, ProductId, SizeId, Quantity)" +
                                        " VALUES (@PurchaseId, @Item, @ProductId, @SizeId, @Quantity);";

                    var count = 0;
                    foreach(var item in purchase.Items.Where(i => i.Quantity > 0))
                    {
                        count++;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);
                        cmd.Parameters.AddWithValue("@Item", count);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@SizeId", item.Size);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Purchase?> Get(SqlConnection connection, int purchaseId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Purchase" +
                                            " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", purchaseId);;

                var reader = await cmd.ExecuteReaderAsync();

                return await reader.ReadAsync() ? 
                    await LoadPurchase(reader, true, connection) : 
                    null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static async Task<Purchase> LoadPurchase(SqlDataReader reader, bool loadItems, SqlConnection? connection)
        {
            var purchase = new Purchase()
            {
                Id = reader.GetInt32("Id"),
                CollectionId = reader.GetInt32("CollectionId"),
                StoreId = reader.GetInt32("StoreId"),
                Status = (PurchaseStatus)reader.GetInt32("Status")
            };

            if (loadItems &&
                connection != null)
            {
                var listItems = new List<PurchaseItem>();

                try
                {
                    await connection.OpenAsync();

                    using var cmd = new SqlCommand("SELECT * FROM Purchase_Product" +
                                                " WHERE PurchaseId = @PurchaseId;", connection);

                    cmd.Parameters.AddWithValue("@PurchaseId", purchase.Id);
                    using var reader2 = await cmd.ExecuteReaderAsync();
                    while (await reader2.ReadAsync())
                        listItems.Add(new PurchaseItem()
                        {
                            ProductId = reader2.GetInt32("ProductId"),
                            Size = reader2.GetString("SizeId"),
                            Quantity = reader2.GetInt32("Quantity")
                        });
                }
                finally
                {
                    await connection.CloseAsync();
                }

                purchase.Items = listItems.ToArray();
            }

            return purchase;
        }

        public static async Task<Purchase?> Get(SqlConnection connection, int collectionId, int storeId, bool loadItems = true)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Purchase" +
                                        " WHERE CollectionId = @CollectionId" +
                                            " AND StoreId = @StoreId;", connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                cmd.Parameters.AddWithValue("@StoreId", storeId);

                var reader = await cmd.ExecuteReaderAsync();

                return await reader.ReadAsync() ?
                    await LoadPurchase(reader, loadItems, connection) :
                    null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Purchase[]> GetPurchases(SqlConnection connection, int collectionId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Purchase" +
                                        " WHERE CollectionId = @CollectionId;", connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);

                var reader = await cmd.ExecuteReaderAsync();
                var list = new List<Purchase>();
                while (await reader.ReadAsync())
                    list.Add(await LoadPurchase(reader, true, connection));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> HasOpenedPurchase(SqlConnection connection, int collectionId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT Id FROM Purchase" +
                                            " WHERE CollectionId = @CollectionId" +
                                                " AND Status = 0;", connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);

                return await cmd.ExecuteScalarAsync() is not null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Reverse(SqlConnection connection, int purchaseId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("UPDATE Purchase" +
                                                " SET Status = 0" +
                                                " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", purchaseId);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
