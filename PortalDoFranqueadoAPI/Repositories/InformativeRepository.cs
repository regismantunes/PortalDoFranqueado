using System.Data;
using PortalDoFranqueadoAPI.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using PortalDoFranqueadoAPI.Extensions;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class InformativeRepository
    {
        private const string RecordNotFoundException = "Não foi encontrado registro de informativo no banco de dados.";

        public static async Task<Informative> Get(SqlConnection connection)
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

        public static async Task Save(SqlConnection connection, Informative informative)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("UPDATE Informative" +
                                                " SET Title = @Title" +
                                                    ", Text = @Text", connection);

                cmd.Parameters.AddWithValue("@Title", informative.Title);
                cmd.Parameters.AddWithValue("@Text", informative.Text);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                {
                    cmd.CommandText = "INSERT INTO Informative (Title, Text) VALUES (@Title, @Text)";
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
