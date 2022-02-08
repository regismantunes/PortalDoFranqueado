using PortalDoFranqueadoAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class MarketingCampaignRepository
    {
        public static async Task<MarketingCampaign[]> GetActiveMarketingCampaigns(SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT * FROM campanha WHERE situacao = 1", connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<MarketingCampaign>();
                while (await reader.ReadAsync())
                    list.Add(new MarketingCampaign()
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("titulo"),
                        FolderId = reader.GetString("pasta"),
                        Status = reader.GetInt32("situacao")
                    });

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
}
