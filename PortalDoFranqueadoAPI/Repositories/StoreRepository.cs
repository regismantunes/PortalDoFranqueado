using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class StoreRepository(SqlConnection connection) : IStoreRepository
    {
        public async Task<IEnumerable<Store>> GetListByUser(int idUser)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT Store.*
                                            FROM Store
                                                INNER JOIN User_Store
                                                    ON Store.Id = User_Store.StoreId
                                            WHERE User_Store.UserId = @UserId;
                                            """, connection);
                cmd.Parameters.AddWithValue("@UserId", idUser);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Store>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        DocumentNumber = reader.GetValue("DocumentNumber") as string
                    });

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<Store>> GetList()
        {
            bool connectionWasClosed = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasClosed = true;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT * FROM Store;", connection);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Store>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        DocumentNumber = reader.GetValue("DocumentNumber") as string
                    });

                await reader.CloseAsync().AsNoContext();

                return list;
            }
            finally
            {
                if (connectionWasClosed)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<Store?> Get(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT *
                                            FROM Store
                                            WHERE Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
                    return new Store()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        DocumentNumber = reader.GetValue("DocumentNumber") as string
                    };

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<int> Insert(Store store)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                INSERT INTO Store (Name, DocumentNumber)
                                                OUTPUT INSERTED.Id
                                                VALUES (@Name, @DocumentNumber);
                                                """, connection);

                cmd.Parameters.AddWithValue("@Name", store.Name);
                cmd.Parameters.AddWithValue("@DocumentNumber", store.DocumentNumber);

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
                                                DELETE FROM Store
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

        public async Task Update(Store store)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                UPDATE Store
                                                    SET Name = @Name
                                                    ,   DocumentNumber = @DocumentNumber
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@Name", store.Name);
                cmd.Parameters.AddWithValue("@DocumentNumber", store.DocumentNumber);
                cmd.Parameters.AddWithValue("@Id", store.Id);

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
