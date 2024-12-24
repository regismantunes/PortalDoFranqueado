using PortalDoFranqueadoAPI.Extensions;
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
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT FileId" +
                                                " FROM Auxiliary_File" +
                                                " WHERE AuxiliaryId = @id", connection);

                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<int>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(reader.GetInt32("FileId"));

                return list.ToArray();
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
