using PortalDoFranqueadoAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class CampaignRepository
    {
        public static async Task<Campaign[]> GetList(SqlConnection connection, bool onlyActives = false)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM Campaign" +
                        ( onlyActives ? " WHERE Status = 1" : string.Empty), connection);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                var list = new List<Campaign>();
                while (await reader.ReadAsync())
                    list.Add(new Campaign()
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        FolderId = reader.GetString("FolderId"),
                        Status = (CampaignStatus)reader.GetInt16("Status")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int> Insert(SqlConnection connection, Campaign campaign)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO Campaign (Title, FolderId, Status)" +
                                            " OUTPUT INSERTED.ID" +
                                            " VALUES (@Title, @FolderId, @Status)", connection);

                cmd.Parameters.AddWithValue("@Title", campaign.Title);
                cmd.Parameters.AddWithValue("@FolderId", campaign.FolderId);
                cmd.Parameters.AddWithValue("@Status", (int)campaign.Status);

                var dbid = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<bool> Delete(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("DELETE FROM Campaign WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) > 0;
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task ChangeStatus(SqlConnection connection, int id, CampaignStatus status)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("UPDATE Campaign" +
                                                " SET Status = @Status" +
                                                " WHERE Id = @Id;", connection);

                cmd.Parameters.AddWithValue("@Status", (int)status);
                cmd.Parameters.AddWithValue("@Id", id);
                
                if (await cmd.ExecuteNonQueryAsync().ConfigureAwait(false) == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
