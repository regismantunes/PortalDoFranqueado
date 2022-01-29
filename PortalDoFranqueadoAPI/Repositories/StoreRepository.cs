﻿using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using System.Data;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class StoreRepository
    {
        public static async Task<Store[]> GetStoresByUser(MySqlConnection connection, int idUser)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT loja.*" +
                                            " FROM loja" +
                                                " INNER JOIN usuario_loja" +
                                                    " ON loja.id = usuario_loja.idloja" +
                                            " WHERE usuario_loja.idusuario = @idusuario;", connection);
                cmd.Parameters.AddWithValue("@idusuario", idUser);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Store>();
                while (await reader.ReadAsync())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("nome")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Store[]> GetStores(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM loja;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Store>();
                while (await reader.ReadAsync())
                    list.Add(new Store()
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("nome")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Store?> Get(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM loja;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return new Store()
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("nome")
                    };

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
