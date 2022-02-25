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

                using var cmd = new SqlCommand("SELECT EndDate FROM [Collection]" +
                                                " WHERE Excluded = 0" +
                                                    " AND Status = 1", connection);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                bool compraHabilitada = await reader.ReadAsync().ConfigureAwait(false);
                var text = "Sem previsão";

                if (compraHabilitada)
                {
                    var fimPeriodo = reader.GetDateTime("EndDate");
                    text = $"Aberto até {fimPeriodo:dd/MM}";
                }
                else
                {
                    await reader.CloseAsync();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT MIN(StartDate) AS NextDate" +
                                    " FROM [Collection]" +
                                    " WHERE Excluded = 0" +
                                        " AND StartDate >= @dataAtual;";
                    cmd.Parameters.AddWithValue("@dataAtual", DateTime.Now);

                    reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        if (reader["NextDate"].GetType() != typeof(DBNull))
                        {
                            var fimPeriodo = reader.GetDateTime("NextDate");
                            text = $"Abre {fimPeriodo:dd/MM}";
                        }
                    }
                }

                return new CollectionInfo()
                {
                    EnabledPurchase = compraHabilitada,
                    TextPurchase = text
                };
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Collection[]> GetCollections(SqlConnection connection, bool onlyActives = true)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM [Collection]" +
                                            " WHERE Excluded = 0" +
                                (onlyActives ? " AND Status IN (0,1);" : string.Empty), connection);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                var list = new List<Collection>();
                while (await reader.ReadAsync())
                    list.Add(CreateCollection(reader));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Collection?> Get(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM [Collection]" +
                                            " WHERE Excluded = 0" +
                                                " AND Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                if (!await reader.ReadAsync().ConfigureAwait(false))
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<bool> HasOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT Id FROM [Collection]" +
                                            " WHERE Excluded = 0" +
                                                " AND Status = 1;", connection);

                return await cmd.ExecuteScalarAsync().ConfigureAwait(false) != null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Collection?> GetOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM [Collection]" +
                                            " WHERE Excluded = 0" +
                                                " AND Status = 1;", connection);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                if (await reader.ReadAsync().ConfigureAwait(false))
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        private static Collection CreateCollection(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                StartDate = reader.GetDateTime("StartDate"),
                EndDate = reader.GetDateTime("EndDate"),
                FolderId = reader["FolderId"].GetType() == typeof(DBNull) ? string.Empty : reader.GetString("FolderId"),
                Status = (CollectionStatus)reader.GetInt16("Status")
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
                    using var cmd = new SqlCommand("SELECT [Status]" +
                                                    " FROM [Collection]" +
                                                    " WHERE Excluded = 0" +
                                                        " AND Id = @Id;", connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Id", id);

                    var previusStatus = (CollectionStatus?)(int?)await cmd.ExecuteScalarAsync();

                    if (previusStatus == null)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    cmd.CommandText = "UPDATE [Collection]" +
                                        $" SET [Status] = {(int)status}" +
                                        " WHERE Excluded = 0" +
                                            " AND Id = @Id;";

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
                        cmd.CommandText = "UPDATE Purchase" +
                                            $" SET [Status] = {(int)purchaseStatus}" +
                                            " WHERE CollectionId = @Id;";

                        await cmd.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync().ConfigureAwait(false);
                }
                catch
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);
                    throw;
                }
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int> Insert(SqlConnection connection, Collection collection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO Collection (StartDate, EndDate, Status, FolderId, Excluded)" +
                                            " OUTPUT INSERTED.ID" +
                                            " VALUES (@StartDate, @EndDate, @Status, @FolderId, 0);", connection);

                cmd.Parameters.AddWithValue("@StartDate", collection.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", collection.EndDate);
                cmd.Parameters.AddWithValue("@FolderId", collection.FolderId);
                cmd.Parameters.AddWithValue("@Status", (int)collection.Status);

                var dbid = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<bool> Delete(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE Collection" +
                                            " SET Excluded = 1" +
                                            " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task Update(SqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE Collection" +
                                            " SET StartDate = @StartDate" +
                                                ", EndDate = @EndDate" +
                                                ", FolderId = @FolderId" +
                                            " WHERE Excluded = 0" +
                                                " AND Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", colecao.Id);
                cmd.Parameters.AddWithValue("@StartDate", colecao.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", colecao.EndDate);
                cmd.Parameters.AddWithValue("@FolderId", colecao.FolderId);

                if (await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}