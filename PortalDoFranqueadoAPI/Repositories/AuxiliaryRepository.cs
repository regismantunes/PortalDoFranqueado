using PortalDoFranqueadoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class AuxiliaryRepository
    {
        public static async Task<int[]> GetIdFiles(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("SELECT FileId" +
                                        " FROM Auxiliary_File" +
                                        " WHERE AuxiliaryId = @id", connection);

                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                var list = new List<int>();
                while (await reader.ReadAsync())
                    list.Add(reader.GetInt32("FileId"));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
