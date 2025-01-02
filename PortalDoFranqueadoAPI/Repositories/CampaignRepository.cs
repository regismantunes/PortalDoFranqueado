using PortalDoFranqueadoAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Enums;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class CampaignRepository(SqlConnection connection, IFileRepository fileRepository) : ICampaignRepository
    {
        public async Task<IEnumerable<Campaign>> GetList(bool onlyActives = false, bool loadFiles = false)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( $"""
                                                SELECT *
                                                FROM Campaign
                                                {(onlyActives ? " WHERE Status = 1" : string.Empty)}
                                                """, connection);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<Campaign>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(new Campaign()
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        Status = (CampaignStatus)reader.GetInt16("Status")
                    });

                await reader.CloseAsync().AsNoContext();

                if (loadFiles)
                    foreach (var campaign in list)
                        campaign.Files = await fileRepository.GetFilesFromCampaign(campaign.Id).AsNoContext();
                
                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<int> Insert(Campaign campaign)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                        INSERT INTO Campaign (Title, Status)
                                        OUTPUT INSERTED.Id
                                        VALUES (@Title, @Status)
                                        """, connection);

                cmd.Parameters.AddWithValue("@Title", campaign.Title);
                cmd.Parameters.AddWithValue("@Status", (int)campaign.Status);

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("DELETE FROM Campaign WHERE Id = @Id", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                return await cmd.ExecuteNonQueryAsync().AsNoContext() > 0;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task ChangeStatus(int id, CampaignStatus status)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                UPDATE Campaign
                                                SET Status = @Status
                                                WHERE Id = @Id
                                                """, connection);

                cmd.Parameters.AddWithValue("@Status", (int)status);
                cmd.Parameters.AddWithValue("@Id", id);
                
                if (await cmd.ExecuteNonQueryAsync().AsNoContext() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
