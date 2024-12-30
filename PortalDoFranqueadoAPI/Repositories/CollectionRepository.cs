using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class CollectionRepository
    {
        public static async Task<CollectionInfo> GetInfo(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                SELECT EndDate
                                                FROM [Collection]
                                                WHERE Excluded = 0
                                                    AND Status = 1
                                                """, connection);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                bool enabledPurchase = await reader.ReadAsync().AsNoContext();
                var text = "Sem previsão";

                if (enabledPurchase)
                {
                    var endDate = reader.GetDateTime("EndDate");
                    text = $"Aberto até {endDate:dd/MM}";
                }
                else
                {
                    await reader.CloseAsync().AsNoContext();

                    cmd.Parameters.Clear();
                    cmd.CommandText =   """
                                        SELECT MIN(StartDate) AS NextDate
                                        FROM [Collection]
                                        WHERE Excluded = 0
                                            AND StartDate >= @dataAtual;
                                        """;
                    cmd.Parameters.AddWithValue("@dataAtual", DateTime.Now);

                    reader = await cmd.ExecuteReaderAsync().AsNoContext();
                    if (await reader.ReadAsync().AsNoContext())
                    {
                        if (reader["NextDate"].GetType() != typeof(DBNull))
                        {
                            var nextDate = reader.GetDateTime("NextDate");
                            text = $"Abre {nextDate:dd/MM}";
                        }
                    }
                }

                return new CollectionInfo()
                {
                    EnabledPurchase = enabledPurchase,
                    TextPurchase = text
                };
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task<Collection[]> GetList(SqlConnection connection, bool onlyActives = true)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   $"""
                                            SELECT * FROM [Collection]
                                            WHERE Excluded = 0
                                            {(onlyActives ? " AND Status IN (0,1);" : string.Empty)}
                                            """, connection);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Collection>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(CreateCollection(reader));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task<Collection?> Get(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            SELECT * FROM [Collection]
                                            WHERE Excluded = 0
                                                AND Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (!await reader.ReadAsync().AsNoContext())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task<bool> HasOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            SELECT Id
                                            FROM [Collection]
                                            WHERE Excluded = 0
                                                AND Status = 1;
                                            """, connection);

                return await cmd.ExecuteScalarAsync().AsNoContext() != null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task<Collection?> GetOpenedCollection(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            SELECT *
                                            FROM [Collection]
                                            WHERE Excluded = 0
                                                AND Status = 1;
                                            """, connection);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        private static Collection CreateCollection(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                StartDate = reader.GetDateTime("StartDate"),
                EndDate = reader.GetDateTime("EndDate"),
                Status = (CollectionStatus)reader.GetInt16("Status")
            };

        public static async Task ChangeStatus(SqlConnection connection, int id, CollectionStatus status)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var transaction = await connection.BeginTransactionAsync().AsNoContext();

                try
                {
                    using var cmd = new SqlCommand("""
                                                    SELECT [Status]
                                                    FROM [Collection]
                                                    WHERE Excluded = 0
                                                        AND Id = @Id;
                                                    """, connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Id", id);

                    var previusStatus = (CollectionStatus?)(short?)await cmd.ExecuteScalarAsync().AsNoContext();

                    if (previusStatus == null)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    cmd.CommandText =   $"""
                                        UPDATE [Collection]
                                            SET [Status] = {(int)status}
                                        WHERE Excluded = 0
                                            AND Id = @Id;
                                        """;

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    var purchaseStatus = (PurchaseStatus?)(previusStatus == CollectionStatus.Closed && status == CollectionStatus.Opened ? PurchaseStatus.Closed :
                                                           previusStatus == CollectionStatus.Opened && status == CollectionStatus.Closed ? PurchaseStatus.Finished :
                                                           null);
                    if (purchaseStatus != null)
                    {
                        cmd.CommandText =   $"""
                                            UPDATE Purchase
                                                SET [Status] = {(int)purchaseStatus}
                                            WHERE CollectionId = @Id;
                                            """;

                        await cmd.ExecuteNonQueryAsync().AsNoContext();
                    }

                    await transaction.CommitAsync().AsNoContext();
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

        public static async Task<int> Insert(SqlConnection connection, Collection collection)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);
                
                var cmd = new SqlCommand(   """
                                            INSERT INTO Collection (StartDate, EndDate, Status, Excluded)
                                            OUTPUT INSERTED.Id
                                            VALUES (@StartDate, @EndDate, @Status, 0);
                                            """, connection);

                cmd.Parameters.AddWithValue("@StartDate", collection.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", collection.EndDate);
                cmd.Parameters.AddWithValue("@Status", (int)collection.Status);

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task<bool> Delete(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            UPDATE Collection
                                                SET Excluded = 1
                                            WHERE Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync().AsNoContext() > 0;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public static async Task Update(SqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            UPDATE Collection
                                                SET StartDate = @StartDate
                                                ,   EndDate = @EndDate
                                            WHERE   Excluded = 0
                                                AND Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@Id", colecao.Id);
                cmd.Parameters.AddWithValue("@StartDate", colecao.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", colecao.EndDate);

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