using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using System.Data;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class CollectionRepository
    {
        public static async Task<CollectionInfo> GetInfo(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT fim FROM colecao" +
                                            " WHERE excluido = 0 AND situacao = 1;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                bool compraHabilitada = reader.Read();
                string texto = "Sem previsão";

                if (compraHabilitada)
                {
                    DateTime fimPeriodo = reader.GetDateTime("fim");
                    texto = $"Aberto até {fimPeriodo:dd/MM}";
                }
                else
                {
                    reader.Close();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT MIN(inicio) AS proximo" +
                                    " FROM colecao" +
                                    " WHERE excluido = 0 AND inicio >= @dataAtual;";
                    cmd.Parameters.AddWithValue("@dataAtual", DateTime.Now);

                    reader = await cmd.ExecuteReaderAsync();
                    if (reader.Read())
                    {
                        if (reader["proximo"].GetType() != typeof(DBNull))
                        {
                            DateTime fimPeriodo = reader.GetDateTime("proximo");
                            texto = $"Abre {fimPeriodo:dd/MM}";
                        }
                    }
                }

                return new CollectionInfo()
                {
                    EnabledPurchase = compraHabilitada,
                    TextPurchase = texto
                };
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection[]> GetCollections(MySqlConnection connection, bool onlyActives = true)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM colecao" +
                                            " WHERE excluido = 0" +
                                (onlyActives ? " AND situacao IN (0,1);" : string.Empty), connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Collection>();
                while (reader.Read())
                    list.Add(CreateCollection(reader));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection?> Get(MySqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM colecao" +
                                                " WHERE excluido = 0" +
                                                    " AND id = @id;", connection);

                cmd.Parameters.Add("id", DbType.Int32).Value = id;

                var reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> HasOpenedCollection(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT id FROM colecao" +
                                            " WHERE excluido = 0" +
                                                " AND situacao = 1;", connection);

                return cmd.ExecuteScalar() is not null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<Collection?> GetOpenedCollection(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM colecao" +
                                            " WHERE excluido = 0" +
                                                " AND situacao = 1;", connection);

                var reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                    return CreateCollection(reader);

                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        private static Collection CreateCollection(MySqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("id"),
                StartDate = reader.GetDateTime("inicio"),
                EndDate = reader.GetDateTime("fim"),
                FolderId = reader["pasta"].GetType() == typeof(DBNull) ? string.Empty : reader.GetString("pasta"),
                Status = (CollectionStatus)reader.GetInt32("situacao")
            };

        public static async Task ChangeCollectionStatus(MySqlConnection connection, int id, CollectionStatus situacao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("UPDATE colecao" +
                                                " SET situacao = @situacao" +
                                                " WHERE excluido = 0" +
                                                    " AND id = @id;", connection);

                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                cmd.Parameters.Add("@situacao", MySqlDbType.Int32).Value = situacao;

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<int> Insert(MySqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("INSERT INTO colecao (inicio, fim, situacao, pasta, excluido)" +
                                                " VALUES (@inicio, @fim, @situacao, @pasta, 0);", connection);

                cmd.Parameters.Add("@inicio", MySqlDbType.DateTime).Value = colecao.StartDate;
                cmd.Parameters.Add("@fim", MySqlDbType.DateTime).Value = colecao.EndDate;
                cmd.Parameters.Add("@pasta", MySqlDbType.String).Value = colecao.FolderId;
                cmd.Parameters.Add("@situacao", MySqlDbType.Int32).Value = colecao.Status;

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception(MessageRepositories.InsertFailException);

                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT LAST_INSERT_ID();";

                var dbid = (ulong)cmd.ExecuteScalar();
                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<bool> Delete(MySqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                //var cmd = new MySqlCommand("DELETE FROM colecao WHERE id = @id;", connection);
                var cmd = new MySqlCommand("UPDATE colecao" +
                                                " SET excluido = 1" +
                                                " WHERE id = @id;", connection);

                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                return cmd.ExecuteNonQuery() > 0;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task Update(MySqlConnection connection, Collection colecao)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("UPDATE colecao" +
                                                " SET inicio = @inicio" +
                                                    ", fim = @fim" +
                                                    ", pasta = @pasta" +
                                                " WHERE excluido = 0" +
                                                    " AND id = @id;", connection);

                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = colecao.Id;
                cmd.Parameters.Add("@inicio", MySqlDbType.DateTime).Value = colecao.StartDate;
                cmd.Parameters.Add("@fim", MySqlDbType.DateTime).Value = colecao.EndDate;
                cmd.Parameters.Add("@pasta", MySqlDbType.VarChar, 33).Value = colecao.FolderId;

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}