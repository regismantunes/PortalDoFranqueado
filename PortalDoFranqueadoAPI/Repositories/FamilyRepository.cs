using PortalDoFranqueadoAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class FamilyRepository
    {
        public static async Task<Family[]> GetList(SqlConnection connection, bool loadSizes)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Family", connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Family>();
                while (await reader.ReadAsync())
                    list.Add(new Family()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name")
                    });

                if (loadSizes)
                {
                    await reader.CloseAsync();

                    cmd.CommandText = "SELECT * FROM Family_Size";
                    reader = await cmd.ExecuteReaderAsync();

                    var tamanhos = new List<KeyValuePair<int, ProductSize>>();
                    while (await reader.ReadAsync())
                        tamanhos.Add(new KeyValuePair<int, ProductSize>(
                            reader.GetInt32("FamilyId"),
                            new ProductSize()
                            {
                                Size = reader.GetString("SizeId"),
                                Order = reader.GetInt16("Order")
                            }));

                    foreach (var familia in list)
                        familia.Sizes = (from item in tamanhos
                                            where item.Key == familia.Id
                                            select item.Value).ToArray();
                }

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
