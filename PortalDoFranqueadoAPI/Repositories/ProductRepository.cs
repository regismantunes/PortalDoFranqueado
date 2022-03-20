using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class ProductRepository
    {
        public static async Task<Product[]> GetList(SqlConnection connection, int collectionId, int? familyId = null)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var list = new List<Product>();
                using (var cmd = new SqlCommand("SELECT * FROM Product" +
                                        " WHERE CollectionId = @CollectionId" +
                    (familyId.HasValue ? " AND FamilyId = @FamilyId" : string.Empty), connection))
                {
                    cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                    if (familyId.HasValue)
                        cmd.Parameters.AddWithValue("@FamilyId", familyId);

                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                        list.Add(new Product()
                        {
                            Id = reader.GetInt32("Id"),
                            FileId = reader.GetInt32("FileId"),
                            Price = reader.GetDecimal("Price"),
                            FamilyId = reader.GetInt32("FamilyId"),
                            LockedSizes = reader.GetStringArray("LockedSizes")
                        });

                    await reader.CloseAsync();
                }

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int> Insert(SqlConnection connection, int collectionId, Product product)
        {
            product.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO Product (CollectionId, FamilyId, FileId, Price, LockedSizes)" +
                                            " OUTPUT INSERTED.Id" +
                                            " VALUES (@CollectionId, @FamilyId, @FileId, @Price, @LockedSizes);", connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@FileId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@LockedSizes", product.LockedSizes.ToDBValue());

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

        public static async Task Update(SqlConnection connection, Product product)
        {
            product.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE Product" +
                                            " SET FamilyId = @FamilyId" +
                                                ", FileId = @FileId" +
                                                ", Price = @Price" +
                                                ", LockedSizes = @LockedSizes" +
                                        " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@FileId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@LockedSizes", product.LockedSizes.ToDBValue());
                cmd.Parameters.AddWithValue("@Id", product.Id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
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

                var transaction = await connection.BeginTransactionAsync();

                try
                {
                    var cmd = new SqlCommand("SELECT FileId" +
                                            " FROM Product" +
                                            " WHERE Id = @Id;", connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Id", id);

                    var fileIdObj = await cmd.ExecuteScalarAsync();

                    cmd.CommandText = "DELETE FROM Product WHERE Id = @Id;";

                    var sucess = await cmd.ExecuteNonQueryAsync() > 0;

                    if (sucess &&
                        fileIdObj is int fileId)
                        await FileRepository.DeleteFile(connection, fileId, transaction);
                    
                    await transaction.CommitAsync().ConfigureAwait(false);

                    return sucess;
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
    }
}
