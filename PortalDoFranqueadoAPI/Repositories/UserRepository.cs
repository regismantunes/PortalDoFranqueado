using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Services;
using System.Data.SqlClient;
using PortalDoFranqueadoAPI.Models.Validations;
using PortalDoFranqueadoAPI.Repositories.Util;

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
                            Role = (UserRole)reader.GetInt16("Role"),
                            Active = true,
                            Treatment = reader.GetString("Treatment")
                        };
                }

                return null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<User[]> GetList(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM [User] WHERE [Status] = 1;", connection);

                var list = new List<User>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(new User()
                        {
                            Id = reader.GetInt32("Id"),
                            Email = reader.GetString("Email"),
                            Name = reader.GetString("Name"),
                            Treatment = reader.GetString("Treatment"),
                            Role = (UserRole)reader.GetInt16("Role"),
                            Active = true
                        });

                    await reader.CloseAsync();
                }

                var stores = await StoreRepository.GetList(connection);

                var dic = new Dictionary<int, List<Store>>();
                cmd.CommandText = "SELECT * FROM User_Store";
                using(var reader = await cmd.ExecuteReaderAsync())
                {
                    while(await reader.ReadAsync())
                    {
                        var userId = reader.GetInt32("UserId");
                        var storeId = reader.GetInt32("StoreId");

                        var storeList = dic.ContainsKey(userId) ? dic[userId] : new List<Store>();
                        storeList.Add(stores.First(store => store.Id == storeId));
                        dic[userId] = storeList;
                    }
                }

                list.ForEach(u => u.Stores = dic.ContainsKey(u.Id) ? dic[u.Id].ToArray() : null);

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int> Insert(SqlConnection connection, User user)
        {
            user.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO [User] (Name, [Status], Email, Password, Role, Treatment)" +
                                            " OUTPUT INSERTED.Id" +
                                            " VALUES (@Name, @Status, @Email, @Password, @Role, @Treatment);", connection);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Status", 1);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@Role", (int)user.Role);

                var dbid = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                var id = Convert.ToInt32(dbid);

                if (user.Stores?.Any() ?? false)
                {
                    cmd.CommandText = "INSERT INTO User_Store (UserId, StoreId) VALUES (@UserId, @StoreId);";
                    foreach (var store in user.Stores)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@UserId", id);
                        cmd.Parameters.AddWithValue("@StoreId", store.Id);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return id;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
