using System.Data;
using PortalDoFranqueadoAPI.Models;
using System.Data.SqlClient;

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

                var cmd = new SqlCommand("SELECT * FROM informativo LIMIT 1", connection);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return new Informative()
                    {
                        Title = reader.GetString("titulo"),
                        Text = reader.GetString("texto")
                    };

                throw new Exception(RecordNotFoundException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Save(SqlConnection connection, Informative informative)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE informativo" +
                                                " SET titulo = @titulo" +
                                                    ", texto = @texto", connection);

                cmd.Parameters.AddWithValue("@titulo", informative.Title);
                cmd.Parameters.AddWithValue("@texto", informative.Text);

                if (await cmd.ExecuteNonQueryAsync() > 0)
                    throw new Exception(RecordNotFoundException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
