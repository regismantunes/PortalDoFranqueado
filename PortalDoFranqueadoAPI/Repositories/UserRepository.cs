using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Services;
using System.Data.SqlClient;
using PortalDoFranqueadoAPI.Models.Validations;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class UserRepository
    {
        public static async Task<(User?,bool)> GetAuthenticated(SqlConnection connection, string username, string password, short resetPasswordMaxAttempts)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var id = default(int);
                var resetPasswordAttempts = default(short);
                var resetPassword = false;
                User? user = null;
                using (var cmd = new SqlCommand("SELECT * FROM [User]" +
                                                " WHERE Email = @username" +
                                                  " AND Status = 1", connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        id = reader.GetInt32("Id");
                        var passwordHash = reader.GetValue("Password") as string;// reader.GetString("Password");
                        if (string.IsNullOrEmpty(passwordHash))
                        {
                            passwordHash = reader.GetValue("ResetPasswordCode") as string;
                            if (string.IsNullOrEmpty(passwordHash))
                                throw new Exception("Solicite ao adminstrador do sistema o código para resetar a senha.");

                            resetPasswordAttempts = reader.GetInt16("ResetPasswordAttempts");
                            if (resetPasswordAttempts > resetPasswordMaxAttempts)
                                throw new Exception("O número máximo de tentativas para resetar a senha foi exedido!");

                            resetPassword = true;
                        }

                        if (HashService.VerifyHash(password, "SHA256", passwordHash))
                        {
                            user = new User()
                            {
                                Email = reader.GetString("Email"),
                                Id = id,
                                Name = reader.GetString("Name"),
                                Role = (UserRole)reader.GetInt16("Role"),
                                Active = true,
                                Treatment = reader.GetValue("Treatment") as string
                            };
                        }

                        await reader.CloseAsync();
                    }
                }

                if (resetPassword)
                {
                    if (user == null)
                    {
                        resetPasswordAttempts++;
                        using var cmdRP = new SqlCommand("UPDATE [User]" +
                                                        " SET ResetPasswordAttempts = @ResetPasswordAttempts" +
                                                        " WHERE Id = @Id", connection);

                        cmdRP.Parameters.AddWithValue("@ResetPasswordAttempts", resetPasswordAttempts);
                        cmdRP.Parameters.AddWithValue("@Id", id);

                        await cmdRP.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        using var cmdRP = new SqlCommand("UPDATE [User]" +
                                                            " SET ResetPasswordCode = NULL" +
                                                               ", ResetPasswordAttempts = NULL" +
                                                            " WHERE Id = @Id", connection);

                        cmdRP.Parameters.AddWithValue("@Id", id);

                        await cmdRP.ExecuteNonQueryAsync();
                    }
                }

                return (user, resetPassword);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<User[]> GetList(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT * FROM [User] WHERE [Status] = 1;", connection);

                var list = new List<User>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        list.Add(new User()
                        {
                            Id = reader.GetInt32("Id"),
                            Email = reader.GetString("Email"),
                            Name = reader.GetString("Name"),
                            Treatment = reader.GetValue("Treatment") as string,
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

                using var cmd = new SqlCommand("INSERT INTO [User] (Name, [Status], Email, Password, Role, Treatment)" +
                                                " OUTPUT INSERTED.Id" +
                                                " VALUES (@Name, @Status, @Email, @Password, @Role, @Treatment);", connection);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Status", 1);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password.ToDBValue());
                cmd.Parameters.AddWithValue("@Role", (int)user.Role);
                cmd.Parameters.AddWithValue("@Treatment", user.Treatment.ToDBValue());

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

        public static async Task<bool> Delete(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("UPDATE [User]" +
                                                " SET [Status] = 0" +
                                                " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task Update(SqlConnection connection, User user)
        {
            user.Validate();

            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var stores = new List<int>();

                using (var cmdStores = new SqlCommand("SELECT StoreId" +
                                                    " FROM User_Store" +
                                                    " WHERE UserId = @Id", connection))
                {
                    cmdStores.Parameters.AddWithValue("@Id", user.Id);

                    using var reader = await cmdStores.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        stores.Add(reader.GetInt32("StoreId"));
                }

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    using var cmd = new SqlCommand("UPDATE [User]" +
                                                    " SET Name = @Name" +
                                                        ", Email = @Email" +
                                                        ", Role = @Role" +
                                                        ", Treatment = @Treatment" +
                                                " WHERE Id = @Id;", connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Role", (int)user.Role);
                    cmd.Parameters.AddWithValue("@Treatment", user.Treatment.ToDBValue());
                    cmd.Parameters.AddWithValue("@Id", user.Id);

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    if (user.Stores?.Any() ?? false)
                    {
                        cmd.CommandText = "INSERT INTO User_Store (UserId, StoreId) VALUES (@UserId, @StoreId);";
                        foreach (var store in user.Stores)
                        {
                            if (!stores.Contains(store.Id))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@UserId", user.Id);
                                cmd.Parameters.AddWithValue("@StoreId", store.Id);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        cmd.Parameters.Clear();
                    }

                    var storesToDelete = stores.Where(s => !(user.Stores?.Any(store => store.Id == s) ?? false));
                    if (storesToDelete.Any())
                    {
                        cmd.CommandText = "DELETE FROM User_Store" +
                                            " WHERE UserId = @UserId" +
                                                " AND StoreId = @StoreId";

                        foreach (var store in storesToDelete)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@UserId", user.Id);
                            cmd.Parameters.AddWithValue("@StoreId", store);

                            await cmd.ExecuteNonQueryAsync();
                        }
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
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<bool> ResetPassword(SqlConnection connection, int id, string resetCode)
        {
            try
            {
                var resetCodeHash = HashService.ComputeHash(resetCode, "SHA256");

                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                 using var cmd = new SqlCommand("UPDATE [User]" +
                                            " SET [Password] = NULL" +
                                               ", ResetPasswordCode = @ResetCode" +
                                               ", ResetPasswordAttempts = 0" +
                                            " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@ResetCode", resetCodeHash);
                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task ChangePassword(SqlConnection connection, int id, string newPassword, string newPasswordConfirmation, string? currentPassword = null)
        {
            try
            {
                if (!newPassword.Equals(newPasswordConfirmation))
                    throw new Exception("A confirmação da senha não confere com a nova senha informada.");

                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT Password" +
                                                " FROM [User]" +
                                                " WHERE Id = @Id" +
                                                  " AND Status = 1", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var passwordHash = reader.GetValue("Password") as string;
                        if (!string.IsNullOrEmpty(passwordHash))
                        {
                            if (string.IsNullOrEmpty(currentPassword))
                                throw new Exception("É necessário informar a senha atual para alterar a senha.");

                            if (!HashService.VerifyHash(currentPassword, "SHA256", passwordHash))
                                throw new Exception("Senha inválida.");
                        }
                    }

                    await reader.CloseAsync();
                }

                var newPasswordHash = HashService.ComputeHash(newPassword, "SHA256");

                cmd.CommandText = "UPDATE [User]" +
                                    " SET Password = @Password" +
                                        ", ResetPasswordCode = NULL" +
                                        ", ResetPasswordAttempts = NULL" +
                                    " WHERE Id = @Id";

                cmd.Parameters.AddWithValue("@Password", newPasswordHash);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
