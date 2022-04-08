using System.Data;
using PortalDoFranqueadoAPI.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class InformativeRepository
    {
        private const string RecordNotFoundException = "Não foi encontrado registro de informativo no banco de dados.";

        public static async Task<Informative> Get(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT TOP 1 * FROM Informative", connection);

                using var reader = await cmd.ExecuteReaderAsync();

                var result = new Informative();

                if (await reader.ReadAsync())
                {
                    result.Title = reader.GetString("Title");
                    result.Text = reader.GetString("Text");
                }

                return result;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task Save(SqlConnection connection, Informative informative)
        {
            try
            {
                await connection.OpenAsync();

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
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
