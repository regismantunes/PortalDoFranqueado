using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;

namespace PortalDoFranqueadoAPI.Repositories
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

                return await LoadPurchase(cmd);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static async Task<Purchase?> LoadPurchase(MySqlCommand cmd, bool loadItems = true)
        {
            var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var purchase = new Purchase()
                {
                    Id = reader.GetInt32("id"),
                    CollectionId = reader.GetInt32("idcolecao"),
                    StoreId = reader.GetInt32("idloja"),
                    Status = (PurchaseStatus)reader.GetInt32("situacao")
                };

                await reader.CloseAsync();

                if (loadItems)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT * FROM compra_produto" +
                                        " WHERE idcompra = @idcompra;";
                    cmd.Parameters.AddWithValue("@idcompra", purchase.Id);

                    reader = await cmd.ExecuteReaderAsync();

                    var listItems = new List<PurchaseItem>();
                    while (await reader.ReadAsync())
                        listItems.Add(new PurchaseItem()
                        {
                            ProductId = reader.GetInt32("idproduto"),
                            Size = reader.GetString("idtamanho"),
                            Quantity = reader.GetInt32("quantidade")
                        });

                    purchase.Items = listItems.ToArray();
                }

                return purchase;
            }

            return null;
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

                return await LoadPurchase(cmd, loadItems);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
