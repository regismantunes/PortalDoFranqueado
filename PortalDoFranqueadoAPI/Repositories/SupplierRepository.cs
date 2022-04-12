using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System;
using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class SupplierRepository
    {
        public static async Task<Supplier[]> GetList(SqlConnection connection, bool onlyActives)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT * FROM Supplier" +
                                    (onlyActives ? "WHERE Active = 1" : string.Empty), connection);

                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Supplier>();
                while (await reader.ReadAsync())
                    list.Add(new Supplier()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Active = reader.GetBoolean("Active")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Supplier?> Get(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT * FROM Supplier WHERE Id = @Id", connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return new Supplier()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Active = reader.GetBoolean("Active")
                    };

                return null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int> Insert(SqlConnection connection, Supplier supplier)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("INSERT INTO Supplier (Name, Active)" +
                                            " OUTPUT INSERTED.Id" +
                                            " VALUES (@Name, @Active);", connection);

                cmd.Parameters.AddWithValue("@Name", supplier.Name);
                cmd.Parameters.AddWithValue("@Active", 1);

                var dbid = await cmd.ExecuteScalarAsync();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
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

                using var cmd = new SqlCommand("DELETE FROM Supplier WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task Update(SqlConnection connection, Supplier supplier)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("UPDATE Supplier" +
                                                " SET Name = @Name" +
                                                    ", Active = @Active" +
                                                " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Name", supplier.Name);
                cmd.Parameters.AddWithValue("@Active", supplier.Active);
                cmd.Parameters.AddWithValue("@Id", supplier.Id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
