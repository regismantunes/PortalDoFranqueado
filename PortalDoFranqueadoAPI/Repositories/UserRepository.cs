using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Services;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class UserRepository
    {
        public static async Task<User?> Get(SqlConnection connection, string username, string password)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM [User]" +
                                            " WHERE Email = @username" +
                                                " AND Status = 1", connection);
                cmd.Parameters.AddWithValue("@username", username);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var passwordHash = reader.GetString("Password");

                    if (HashService.VerifyHash(password, "SHA256", passwordHash))
                        return new User()
                        {
                            Email = reader.GetString("Email"),
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Role = reader.GetInt16("Role") == 9 ? "manager" : "low",
                            Active = true,
                            Treatment = reader.GetString("Treatment")
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
