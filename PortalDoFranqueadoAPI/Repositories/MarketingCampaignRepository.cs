using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using System.Data;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class MarketingCampaignRepository
    {
        public static async Task<MarketingCampaign[]> GetActiveMarketingCampaigns(MySqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new MySqlCommand("SELECT * FROM campanha WHERE situacao = 1", connection);

                var reader = await cmd.ExecuteReaderAsync();

                var list = new List<MarketingCampaign>();
                while (reader.Read())
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
