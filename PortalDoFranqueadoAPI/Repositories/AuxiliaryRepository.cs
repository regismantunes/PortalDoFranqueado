using PortalDoFranqueadoAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class AuxiliaryRepository(SqlConnection connection) : IAuxiliaryRepository
    {
        public async Task<IEnumerable<int>> GetIdFiles(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("""
                                                SELECT FileId
                                                FROM Auxiliary_File
                                                WHERE AuxiliaryId = @id
                                                """, connection);

                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                var list = new List<int>();
                while (await reader.ReadAsync().AsNoContext())
                    list.Add(reader.GetInt32("FileId"));

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
