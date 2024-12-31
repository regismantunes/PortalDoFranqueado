using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class SupplierRepository(SqlConnection connection) : ISupplierRepository
    {
        public async Task<IEnumerable<Supplier>> GetList(bool onlyActives)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand($"""
                                                SELECT * FROM Supplier
                                                {(onlyActives ? " WHERE Active = 1" : string.Empty)}
                                                """, connection);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Supplier>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(new Supplier()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Active = reader.GetBoolean("Active")
                    });

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<Supplier?> Get(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                SELECT *
                                                FROM Supplier
                                                WHERE Id = @Id
                                                """, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
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
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<int> Insert(Supplier supplier)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                INSERT INTO Supplier (Name, Active)
                                                OUTPUT INSERTED.Id
                                                VALUES (@Name, @Active);
                                                """, connection);

                cmd.Parameters.AddWithValue("@Name", supplier.Name);
                cmd.Parameters.AddWithValue("@Active", 1);

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
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
                                                DELETE FROM Supplier
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task Update(Supplier supplier)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                UPDATE Supplier
                                                    SET Name = @Name
                                                    ,   Active = @Active
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@Name", supplier.Name);
                cmd.Parameters.AddWithValue("@Active", supplier.Active);
                cmd.Parameters.AddWithValue("@Id", supplier.Id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
