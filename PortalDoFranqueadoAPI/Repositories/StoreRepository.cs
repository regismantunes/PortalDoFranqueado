using PortalDoFranqueadoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class StoreRepository
    {
        public static async Task<Store[]> GetListByUser(SqlConnection connection, int idUser)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT Store.*" +
                                            " FROM Store" +
                                                " INNER JOIN User_Store" +
                                                    " ON Store.Id = User_Store.StoreId" +
                                            " WHERE User_Store.UserId = @UserId;", connection);
                cmd.Parameters.AddWithValue("@UserId", idUser);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Store>();
                while (await reader.ReadAsync())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("Name")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Store[]> GetList(SqlConnection connection)
        {
            bool connectionWasClosed = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasClosed = true;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT * FROM Store;", connection);

                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Store>();
                while (await reader.ReadAsync())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name")
                    });

                await reader.CloseAsync();

                return list.ToArray();
            }
            finally
            {
                if (connectionWasClosed)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<Store?> Get(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Store" +
                                        " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return new Store()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name")
                    };

                return null;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
