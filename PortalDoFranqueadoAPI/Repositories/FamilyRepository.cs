using PortalDoFranqueadoAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class FamilyRepository(SqlConnection connection) : IFamilyRepository
    {
        public async Task<IEnumerable<Family>> GetList(bool loadSizes)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Family", connection);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Family>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(new Family()
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name")
                    });

                if (loadSizes)
                {
                    await reader.CloseAsync().AsNoContext();

                    cmd.CommandText = "SELECT * FROM Family_Size";
                    reader = await cmd.ExecuteReaderAsync().AsNoContext();

                    var tamanhos = new List<KeyValuePair<int, ProductSize>>();
                    while (await reader.ReadAsync().AsNoContext())
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

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
