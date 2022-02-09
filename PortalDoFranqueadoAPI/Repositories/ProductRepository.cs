using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class ProductRepository
    {
        public static async Task<Product[]> GetProducts(SqlConnection connection, int collectionId, int? familyId = null)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Product" +
                                        " WHERE CollectionId = @CollectionId" +
                    (familyId.HasValue ? " AND FamilyId = @FamilyId" : string.Empty), connection);
                
                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                if (familyId.HasValue)
                    cmd.Parameters.AddWithValue("@FamilyId", familyId);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Product>();
                while (await reader.ReadAsync())
                    list.Add(new Product()
                    {
                        Id = reader.GetInt32("Id"),
                        FileId = reader.GetString("PhotoId"),
                        Price = reader.GetDecimal("Price"),
                        FamilyId = reader.GetInt32("FamilyId")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
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

                var cmd = new SqlCommand("INSERT INTO Product (CollectionId, FamilyId, PhotoId, Price)" +
                                                " VALUES (@CollectionId, @FamilyId, @PhotoId, @Price);", connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@PhotoId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.InsertFailException);

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT SCOPE_IDENTITY();";

                var newid = (ulong)await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(newid);
            }
            finally
            {
                await connection.CloseAsync();
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
                                                ", PhotoId = @PhotoId" +
                                                ", Price = @Price" +
                                        " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@PhotoId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@Id", product.Id);
                
                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
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

                var cmd = new SqlCommand("DELETE FROM Product" +
                                        " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
