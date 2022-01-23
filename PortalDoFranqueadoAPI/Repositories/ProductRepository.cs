using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class ProductRepository
    {
        public static async Task<Product[]> GetProducts(MySqlConnection connection, int collectionId, int? familyId = null)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM produto" +
                                        " WHERE idcolecao = @idcolecao" +
                    (familyId.HasValue ? " AND idfamilia = @idfamilia" : string.Empty), connection);
                
                cmd.Parameters.Add("@idcolecao", DbType.Int32).Value = collectionId;
                if (familyId.HasValue)
                    cmd.Parameters.Add("@idfamilia", DbType.Int32).Value = familyId;

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Product>();
                while (reader.Read())
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

        public static async Task<int> Insert(MySqlConnection connection, int collectionId, Product product)
        {
            product.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("INSERT INTO produto (idcolecao, idfamilia, foto, preco)" +
                                                " VALUES (@idcolecao, @idfamilia, @foto, @preco);", connection);

                cmd.Parameters.Add("@idcolecao", MySqlDbType.Int32).Value = collectionId;
                cmd.Parameters.Add("@idfamilia", MySqlDbType.Int32).Value = product.FamilyId.ToDBValue();
                cmd.Parameters.Add("@foto", MySqlDbType.String).Value = product.FileId.ToDBValue();
                cmd.Parameters.Add("@preco", MySqlDbType.Decimal).Value = product.Price.ToDBValue();

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception(MessageRepositories.InsertFailException);

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT LAST_INSERT_ID();";

                var newid = (ulong)cmd.ExecuteScalar();
                return Convert.ToInt32(newid);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Update(MySqlConnection connection, Product product)
        {
            product.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("UPDATE produto" +
                                            " SET idfamilia = @idfamilia" +
                                                ", foto = @foto" +
                                                ", preco = @preco" +
                                        " WHERE id = @id;", connection);

                cmd.Parameters.Add("@idfamilia", MySqlDbType.Int32).Value = product.FamilyId.ToDBValue();
                cmd.Parameters.Add("@foto", MySqlDbType.String).Value = product.FileId.ToDBValue();
                cmd.Parameters.Add("@preco", MySqlDbType.Decimal).Value = product.Price.ToDBValue();
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = product.Id;

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> Delete(MySqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("DELETE FROM produto" +
                                        " WHERE id = @id;", connection);

                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                return cmd.ExecuteNonQuery() > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
