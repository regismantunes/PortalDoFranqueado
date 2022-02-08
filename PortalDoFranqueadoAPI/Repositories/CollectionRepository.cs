using PortalDoFranqueadoAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class CollectionRepository
    {
        public static async Task<CollectionInfo> GetInfo(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT fim FROM colecao" +
                                                " WHERE excluido = 0" +
                                                    " AND situacao = 1;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                bool compraHabilitada = await reader.ReadAsync();
                string texto = "Sem previsão";

                if (compraHabilitada)
                {
                    var fimPeriodo = reader.GetDateTime("fim");
                    texto = $"Aberto até {fimPeriodo:dd/MM}";
                }
                else
                {
                    await reader.CloseAsync();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT MIN(inicio) AS proximo" +
                                    " FROM colecao" +
                                    " WHERE excluido = 0" +
                                        " AND inicio >= @dataAtual;";
                    cmd.Parameters.AddWithValue("@dataAtual", DateTime.Now);

                    reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        if (reader["proximo"].GetType() != typeof(DBNull))
                        {
                            var fimPeriodo = reader.GetDateTime("proximo");
                            texto = $"Abre {fimPeriodo:dd/MM}";
                        }
                    }
                }

                return new CollectionInfo()
                {
                    EnabledPurchase = compraHabilitada,
                    TextPurchase = texto
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection[]> GetCollections(SqlConnection connection, bool onlyActives = true)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM colecao" +
                                            " WHERE excluido = 0" +
                                (onlyActives ? " AND situacao IN (0,1);" : string.Empty), connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Collection>();
                while (await reader.ReadAsync())
                    list.Add(CreateCollection(reader));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection?> Get(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM colecao" +
                                            " WHERE excluido = 0" +
                                                " AND id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> HasOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT id FROM colecao" +
                                            " WHERE excluido = 0" +
                                                " AND situacao = 1;", connection);

                return await cmd.ExecuteScalarAsync() is not null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection?> GetOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM colecao" +
                                            " WHERE excluido = 0" +
                                                " AND situacao = 1;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static Collection CreateCollection(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("id"),
                StartDate = reader.GetDateTime("inicio"),
                EndDate = reader.GetDateTime("fim"),
                FolderId = reader["pasta"].GetType() == typeof(DBNull) ? string.Empty : reader.GetString("pasta"),
                Status = (CollectionStatus)reader.GetInt32("situacao")
            };

        public static async Task ChangeStatus(SqlConnection connection, int id, CollectionStatus status)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var transaction = await connection.BeginTransactionAsync();

                try
                {
                    using var cmd = new SqlCommand("SELECT situacao" +
                                                    " FROM colecao" +
                                                    " WHERE excluido = 0" +
                                                        " AND id = @id;", connection);

                    cmd.Parameters.AddWithValue("@id", id);

                    var previusStatus = (CollectionStatus?)await cmd.ExecuteScalarAsync();

                    if (previusStatus == null)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    cmd.CommandText = "UPDATE colecao" +
                                        $" SET situacao = {(int)status}" +
                                        " WHERE excluido = 0" +
                                            " AND id = @id;";

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    var purchaseStatus = (PurchaseStatus?)(previusStatus == CollectionStatus.Closed &&
                                                           status == CollectionStatus.Opened ?
                                                            PurchaseStatus.Closed :
                                                           previusStatus == CollectionStatus.Opened &&
                                                           status == CollectionStatus.Closed ?
                                                            PurchaseStatus.Finished :
                                                            null);
                    if (purchaseStatus != null)
                    {
                        cmd.CommandText = "UPDATE compra" +
                                            $" SET situacao = {(int)purchaseStatus}" +
                                            " WHERE idcolecao = @id;";

                        await cmd.ExecuteNonQueryAsync();
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

        public static async Task<int> Insert(SqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO colecao (inicio, fim, situacao, pasta, excluido)" +
                                                " VALUES (@inicio, @fim, @situacao, @pasta, 0);", connection);

                cmd.Parameters.AddWithValue("@inicio", colecao.StartDate);
                cmd.Parameters.AddWithValue("@fim", colecao.EndDate);
                cmd.Parameters.AddWithValue("@pasta", colecao.FolderId);
                cmd.Parameters.AddWithValue("@situacao", (int)colecao.Status);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.InsertFailException);

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT LAST_INSERT_ID();";

                var dbid = (ulong)await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> Delete(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE colecao" +
                                            " SET excluido = 1" +
                                            " WHERE id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Update(SqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE colecao" +
                                            " SET inicio = @inicio" +
                                                ", fim = @fim" +
                                                ", pasta = @pasta" +
                                            " WHERE excluido = 0" +
                                                " AND id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", colecao.Id);
                cmd.Parameters.AddWithValue("@inicio", colecao.StartDate);
                cmd.Parameters.AddWithValue("@fim", colecao.EndDate);
                cmd.Parameters.AddWithValue("@pasta", colecao.FolderId);

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