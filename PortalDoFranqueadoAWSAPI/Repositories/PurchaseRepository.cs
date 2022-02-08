using MySqlConnector;
using PortalDoFranqueadoAPIAWS.Models;
using System.Data;
using PortalDoFranqueadoAPIAWS.Models.Validations;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PortalDoFranqueadoAPIAWS.Repositories
{
    public static class PurchaseRepository
    {
        public static async Task Save(MySqlConnection connection, Purchase purchase)
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
                    using var cmd = new MySqlCommand()
                    {
                        Connection = connection,
                        CommandText = newPurchase ?
                                    "INSERT INTO compra (idloja, idcolecao, situacao)" +
                                        " VALUES (@idloja, @idcolecao, @situacao);" :
                                    "UPDATE compra" +
                                        " SET idloja = @idloja" +
                                            ", idcolecao = @idcolecao" +
                                            ", situacao = @situacao" +
                                        " WHERE id = @id;",
                        Transaction = transaction
                    };

                    cmd.Parameters.AddWithValue("@idloja", purchase.StoreId);
                    cmd.Parameters.AddWithValue("@idcolecao", purchase.CollectionId);
                    cmd.Parameters.AddWithValue("@situacao", (int)purchase.Status);
                    if (!newPurchase)
                        cmd.Parameters.AddWithValue("@id", purchase.Id);

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    if (newPurchase)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT LAST_INSERT_ID();";

                        var dbid = (ulong)await cmd.ExecuteScalarAsync();
                        purchase.Id = Convert.ToInt32(dbid);
                    }
                    else
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "DELETE FROM compra_produto" +
                                        " WHERE idcompra = @idcompra;";
                        cmd.Parameters.AddWithValue("@idcompra", purchase.Id);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    cmd.CommandText = "INSERT INTO compra_produto (idcompra, item, idproduto, idtamanho, quantidade)" +
                                        " VALUES (@idcompra, @item, @idproduto, @idtamanho, @quantidade);";

                    var count = 0;
                    foreach(var item in purchase.Items.Where(i => i.Quantity > 0))
                    {
                        count++;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@idcompra", purchase.Id);
                        cmd.Parameters.AddWithValue("@item", count);
                        cmd.Parameters.AddWithValue("@idproduto", item.ProductId);
                        cmd.Parameters.AddWithValue("@idtamanho", item.Size);
                        cmd.Parameters.AddWithValue("@quantidade", item.Quantity);

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

        public static async Task<Purchase?> Get(MySqlConnection connection, int purchaseId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM compra" +
                                            " WHERE id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", purchaseId);;

                var reader = await cmd.ExecuteReaderAsync();

                return await reader.ReadAsync() ? 
                    await LoadPurchase(reader, true, connection.Clone()) : 
                    null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static async Task<Purchase> LoadPurchase(MySqlDataReader reader, bool loadItems, MySqlConnection? connection)
        {
            var purchase = new Purchase()
            {
                Id = reader.GetInt32("id"),
                CollectionId = reader.GetInt32("idcolecao"),
                StoreId = reader.GetInt32("idloja"),
                Status = (PurchaseStatus)reader.GetInt32("situacao")
            };

            if (loadItems &&
                connection != null)
            {
                var listItems = new List<PurchaseItem>();

                try
                {
                    await connection.OpenAsync();

                    using var cmd = new MySqlCommand("SELECT * FROM compra_produto" +
                                        " WHERE idcompra = @idcompra;", connection);

                    cmd.Parameters.AddWithValue("@idcompra", purchase.Id);
                    using var reader2 = await cmd.ExecuteReaderAsync();
                    while (await reader2.ReadAsync())
                        listItems.Add(new PurchaseItem()
                        {
                            ProductId = reader2.GetInt32("idproduto"),
                            Size = reader2.GetString("idtamanho"),
                            Quantity = reader2.GetInt32("quantidade")
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

        public static async Task<Purchase?> Get(MySqlConnection connection, int collectionId, int storeId, bool loadItems = true)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM compra" +
                                        " WHERE idcolecao = @idcolecao" +
                                            " AND idloja = @idloja;", connection);

                cmd.Parameters.AddWithValue("@idcolecao", collectionId);
                cmd.Parameters.AddWithValue("@idloja", storeId);

                var reader = await cmd.ExecuteReaderAsync();

                return await reader.ReadAsync() ?
                    await LoadPurchase(reader, loadItems, connection.Clone()) :
                    null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Purchase[]> GetPurchases(MySqlConnection connection, int collectionId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM compra" +
                                        " WHERE idcolecao = @idcolecao;", connection);

                cmd.Parameters.AddWithValue("@idcolecao", collectionId);

                var reader = await cmd.ExecuteReaderAsync();
                var list = new List<Purchase>();
                while (await reader.ReadAsync())
                    list.Add(await LoadPurchase(reader, true, connection.Clone()));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> HasOpenedPurchase(MySqlConnection connection, int collectionId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT id FROM compra" +
                                            " WHERE idcolecao = @idcolecao" +
                                                " AND situacao = 0;", connection);

                cmd.Parameters.AddWithValue("@idcolecao", collectionId);

                return await cmd.ExecuteScalarAsync() is not null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Reverse(MySqlConnection connection, int purchaseId)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new MySqlCommand("UPDATE compra" +
                                                " SET situacao = 0" +
                                                " WHERE id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", purchaseId);

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
