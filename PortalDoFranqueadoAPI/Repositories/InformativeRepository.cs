using System.Data;
using PortalDoFranqueadoAPI.Models;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class InformativeRepository(SqlConnection connection) : IInformativeRepository
    {
        public async Task<Informative> Get()
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT TOP 1 * FROM Informative", connection);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var result = new Informative();

                if (await reader.ReadAsync().AsNoContext())
                {
                    result.Title = reader.GetString("Title");
                    result.Text = reader.GetString("Text");
                }

                return result;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task Save(Informative informative)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                UPDATE Informative
                                                    SET Title = @Title
                                                    ,   Text = @Text
                                                """, connection);

                cmd.Parameters.AddWithValue("@Title", informative.Title);
                cmd.Parameters.AddWithValue("@Text", informative.Text);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    cmd.CommandText =   """
                                        INSERT INTO Informative (Title, Text)
                                        VALUES (@Title, @Text)
                                        """;
                    await cmd.ExecuteNonQueryAsync().AsNoContext();
                }
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
