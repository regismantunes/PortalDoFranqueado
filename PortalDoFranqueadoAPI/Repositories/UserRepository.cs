using PortalDoFranqueadoAPI.Models;
using System.Data;
using PortalDoFranqueadoAPI.Services;
using Microsoft.Data.SqlClient;
using PortalDoFranqueadoAPI.Models.Validations;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Enums;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class UserRepository(SqlConnection connection, IStoreRepository storeRepository) : IUserRepository
    {
        public async Task<(User?, bool)> GetAuthenticated(string username, string password, short resetPasswordMaxAttempts)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var id = default(int);
                var resetPasswordAttempts = default(short);
                var resetPassword = false;
                User? user = null;
                using (var cmd = new SqlCommand("""
                                                SELECT *
                                                FROM [User]
                                                WHERE Email = @username
                                                    AND Status = 1
                                                """, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                    if (await reader.ReadAsync().AsNoContext())
                    {
                        id = reader.GetInt32("Id");
                        var passwordHash = reader.GetValue("Password") as string;
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

                        await reader.CloseAsync().AsNoContext();
                    }
                }

                if (resetPassword)
                {
                    if (user == null)
                    {
                        resetPasswordAttempts++;
                        using var cmdRP = new SqlCommand("""
                                                            UPDATE [User]
                                                                SET ResetPasswordAttempts = @ResetPasswordAttempts
                                                            WHERE Id = @Id
                                                            """, connection);

                        cmdRP.Parameters.AddWithValue("@ResetPasswordAttempts", resetPasswordAttempts);
                        cmdRP.Parameters.AddWithValue("@Id", id);

                        await cmdRP.ExecuteNonQueryAsync().AsNoContext();
                    }
                    else
                    {
                        using var cmdRP = new SqlCommand("""
                                                            UPDATE [User]
                                                                SET ResetPasswordCode = NULL
                                                                ,   ResetPasswordAttempts = NULL
                                                            WHERE Id = @Id
                                                            """, connection);

                        cmdRP.Parameters.AddWithValue("@Id", id);

                        await cmdRP.ExecuteNonQueryAsync().AsNoContext();
                    }
                }

                return (user, resetPassword);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<User>> GetList()
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                SELECT *
                                                FROM [User]
                                                WHERE [Status] = 1;
                                                """, connection);

                var list = new List<User>();
                using (var reader = await cmd.ExecuteReaderAsync().AsNoContext())
                {
                    while (await reader.ReadAsync().AsNoContext())
                        list.Add(new User()
                        {
                            Id = reader.GetInt32("Id"),
                            Email = reader.GetString("Email"),
                            Name = reader.GetString("Name"),
                            Treatment = reader.GetValue("Treatment") as string,
                            Role = (UserRole)reader.GetInt16("Role"),
                            Active = true
                        });

                    await reader.CloseAsync().AsNoContext();
                }

                var stores = await storeRepository.GetList().AsNoContext();

                var dic = new Dictionary<int, List<Store>>();
                cmd.CommandText = """
                                    SELECT *
                                    FROM User_Store
                                    """;
                using (var reader = await cmd.ExecuteReaderAsync().AsNoContext())
                {
                    while (await reader.ReadAsync().AsNoContext())
                    {
                        var userId = reader.GetInt32("UserId");
                        var storeId = reader.GetInt32("StoreId");

                        var storeList = dic.TryGetValue(userId, out List<Store>? value) ? value : [];
                        storeList.Add(stores.First(store => store.Id == storeId));
                        dic[userId] = storeList;
                    }
                }

                list.ForEach(u => u.Stores = dic.TryGetValue(u.Id, out List<Store>? value) ? value.ToArray() : null);

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<int> Insert(User user)
        {

            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                INSERT INTO [User]
                                                    (   Name
                                                    ,   [Status]
                                                    ,   Email
                                                    ,   Password
                                                    ,   Role
                                                    ,   Treatment)
                                                OUTPUT INSERTED.Id
                                                VALUES (@Name
                                                    ,   @Status
                                                    ,   @Email
                                                    ,   @Password
                                                    ,   @Role
                                                    ,   @Treatment);
                                                """, connection);

                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Status", 1);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password.ToDBValue());
                cmd.Parameters.AddWithValue("@Role", (int)user.Role);
                cmd.Parameters.AddWithValue("@Treatment", user.Treatment.ToDBValue());

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                var id = Convert.ToInt32(dbid);

                if (user.Stores?.Any() ?? false)
                {
                    cmd.CommandText = """
                                        INSERT INTO User_Store
                                            (   UserId
                                            ,   StoreId) 
                                        VALUES (@UserId
                                            ,   @StoreId);
                                        """;
                    foreach (var store in user.Stores)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@UserId", id);
                        cmd.Parameters.AddWithValue("@StoreId", store.Id);

                        await cmd.ExecuteNonQueryAsync().AsNoContext();
                    }
                }

                return id;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                UPDATE [User]
                                                    SET [Status] = 0
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

        public async Task Update(User user)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var stores = new List<int>();

                using (var cmdStores = new SqlCommand("""
                                                        SELECT StoreId
                                                        FROM User_Store
                                                        WHERE UserId = @Id
                                                        """, connection))
                {
                    cmdStores.Parameters.AddWithValue("@Id", user.Id);

                    using var reader = await cmdStores.ExecuteReaderAsync().AsNoContext();
                    while (await reader.ReadAsync().AsNoContext())
                        stores.Add(reader.GetInt32("StoreId"));
                }

                using var transaction = await connection.BeginTransactionAsync().AsNoContext();
                try
                {
                    using var cmd = new SqlCommand("""
                                                    UPDATE [User] 
                                                        SET Name = @Name 
                                                        ,   Email = @Email 
                                                        ,   Role = @Role 
                                                        ,   Treatment = @Treatment 
                                                    WHERE Id = @Id;
                                                    """, connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Role", (int)user.Role);
                    cmd.Parameters.AddWithValue("@Treatment", user.Treatment.ToDBValue());
                    cmd.Parameters.AddWithValue("@Id", user.Id);

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    if (user.Stores?.Any() ?? false)
                    {
                        cmd.CommandText = """
                                            INSERT INTO User_Store (UserId, StoreId)
                                            VALUES (@UserId, @StoreId);
                                            """;
                        foreach (var store in user.Stores)
                        {
                            if (!stores.Contains(store.Id))
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@UserId", user.Id);
                                cmd.Parameters.AddWithValue("@StoreId", store.Id);

                                await cmd.ExecuteNonQueryAsync().AsNoContext();
                            }
                        }

                        cmd.Parameters.Clear();
                    }

                    var storesToDelete = stores.Where(s => !(user.Stores?.Any(store => store.Id == s) ?? false));
                    if (storesToDelete.Any())
                    {
                        cmd.CommandText = """
                                            DELETE FROM User_Store
                                            WHERE UserId = @UserId
                                                AND StoreId = @StoreId
                                            """;

                        foreach (var store in storesToDelete)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@UserId", user.Id);
                            cmd.Parameters.AddWithValue("@StoreId", store);

                            await cmd.ExecuteNonQueryAsync().AsNoContext();
                        }
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

        public async Task<bool> ResetPassword(int id, string resetCode)
        {
            try
            {
                var resetCodeHash = HashService.ComputeHash(resetCode, "SHA256");

                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                UPDATE [User]
                                                    SET [Password] = NULL
                                                    ,   ResetPasswordCode = @ResetCode
                                                    ,   ResetPasswordAttempts = 0
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@ResetCode", resetCodeHash);
                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync().AsNoContext() > 0;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task ChangePassword(int id, string newPassword, string newPasswordConfirmation, string? currentPassword = null)
        {
            const string HashAlgorithm = "SHA256";

            try
            {
                if (!newPassword.Equals(newPasswordConfirmation))
                    throw new Exception("A confirmação da senha não confere com a nova senha informada.");

                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                SELECT Password
                                                FROM [User]
                                                WHERE Id = @Id
                                                    AND Status = 1
                                                """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = await cmd.ExecuteReaderAsync().AsNoContext())
                {
                    if (await reader.ReadAsync().AsNoContext())
                    {
                        var passwordHash = reader.GetValue("Password") as string;
                        if (!string.IsNullOrEmpty(passwordHash))
                        {
                            if (string.IsNullOrEmpty(currentPassword))
                                throw new Exception("É necessário informar a senha atual para alterar a senha.");

                            if (!HashService.VerifyHash(currentPassword, HashAlgorithm, passwordHash))
                                throw new Exception("Senha inválida.");
                        }
                    }

                    await reader.CloseAsync().AsNoContext();
                }

                var newPasswordHash = HashService.ComputeHash(newPassword, HashAlgorithm);

                cmd.CommandText = """
                                    UPDATE [User]
                                        SET Password = @Password
                                        ,   ResetPasswordCode = NULL
                                        ,   ResetPasswordAttempts = NULL
                                    WHERE Id = @Id
                                    """;

                cmd.Parameters.AddWithValue("@Password", newPasswordHash);

                await cmd.ExecuteNonQueryAsync().AsNoContext();
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
