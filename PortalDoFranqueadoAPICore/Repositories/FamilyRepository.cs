using MySqlConnector;
using PortalDoFranqueadoAPICore.Models;
using System.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PortalDoFranqueadoAPICore.Repositories
{
    public static class FamilyRepository
    {
        public static async Task<Family[]> GetFamilies(MySqlConnection connection, bool loadSizes)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM familia", connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Family>();
                while (await reader.ReadAsync())
                    list.Add(new Family()
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("nome")
                    });

                if (loadSizes)
                {
                    await reader.CloseAsync();

                    cmd.CommandText = "SELECT * FROM familia_tamanho";
                    reader = await cmd.ExecuteReaderAsync();

                    var tamanhos = new List<KeyValuePair<int, string>>();
                    while (await reader.ReadAsync())
                        tamanhos.Add(new KeyValuePair<int, string>(
                            reader.GetInt32("idfamilia"),
                            reader.GetString("idtamanho")));

                    foreach (var familia in list)
                        familia.Sizes = (from item in tamanhos
                                            where item.Key == familia.Id
                                            select item.Value).ToArray();
                }

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
