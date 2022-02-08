using MySqlConnector;
using System.Data;
using PortalDoFranqueadoAPIAWS.Models;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPIAWS.Repositories
{
    public static class InformativeRepository
    {
        private const string RecordNotFoundException = "Não foi encontrado registro de informativo no banco de dados.";

        public static async Task<Informative> Get(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM informativo LIMIT 1", connection);

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

        public static async Task Save(MySqlConnection connection, Informative informative)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("UPDATE informativo" +
                                                " SET titulo = @titulo" +
                                                    ", texto = @texto", connection);

                cmd.Parameters.Add("@titulo", MySqlDbType.String).Value = informative.Title;
                cmd.Parameters.Add("@texto", MySqlDbType.String).Value = informative.Text;

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
