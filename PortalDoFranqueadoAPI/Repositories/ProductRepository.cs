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

                var cmd = new SqlCommand("SELECT * FROM produto" +
                                        " WHERE idcolecao = @idcolecao" +
                    (familyId.HasValue ? " AND idfamilia = @idfamilia" : string.Empty), connection);
                
                cmd.Parameters.AddWithValue("@idcolecao", collectionId);
                if (familyId.HasValue)
                    cmd.Parameters.AddWithValue("@idfamilia", familyId);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Product>();
                while (await reader.ReadAsync())
                    list.Add(new Product()
                    {
                        Id = reader.GetInt32("id"),
                        FileId = reader.GetString("foto"),
                        Price = reader.GetDecimal("preco"),
                        FamilyId = reader.GetInt32("idfamilia")
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

                var cmd = new SqlCommand("INSERT INTO produto (idcolecao, idfamilia, foto, preco)" +
                                                " VALUES (@idcolecao, @idfamilia, @foto, @preco);", connection);

                cmd.Parameters.AddWithValue("@idcolecao", collectionId);
                cmd.Parameters.AddWithValue("@idfamilia", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@foto", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@preco", product.Price.ToDBValue());

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.InsertFailException);

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT LAST_INSERT_ID();";

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

                var cmd = new SqlCommand("UPDATE produto" +
                                            " SET idfamilia = @idfamilia" +
                                                ", foto = @foto" +
                                                ", preco = @preco" +
                                        " WHERE id = @id;", connection);

                cmd.Parameters.AddWithValue("@idfamilia", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@foto", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@preco", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@id", product.Id);
                
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

                var cmd = new SqlCommand("DELETE FROM produto" +
                                        " WHERE id = @id;", connection);

                cmd.Parameters.AddWithValue("@id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
