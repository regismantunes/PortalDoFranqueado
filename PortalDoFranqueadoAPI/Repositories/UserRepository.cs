using PortalDoFranqueadoAPI.Models;
using MySqlConnector;
using System.Data;
using PortalDoFranqueadoAPI.Services;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class UserRepository
    {
        public static async Task<User?> Get(MySqlConnection connection, string username, string password)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM usuario" +
                                            " WHERE email = @username" +
                                                " AND situacao = 1", connection);
                cmd.Parameters.AddWithValue("@username", username);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var passwordHash = reader.GetString("senha");

                    if (HashService.VerifyHash(password, "SHA256", passwordHash))
                        return new User()
                        {
                            Email = reader.GetString("email"),
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("nome"),
                            Role = reader.GetInt16("nivel") == 9 ? "manager" : "low",
                            Active = true,
                            Treatment = reader.GetString("tratamento")
                        };
                }

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
