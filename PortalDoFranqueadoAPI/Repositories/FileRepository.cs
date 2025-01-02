using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class FileRepository(SqlConnection connection) : IFileRepository
    {
        public async Task<MyFile> GetFile(int id)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                SELECT  Id
                                                    ,   CreatedDate
                                                    ,   [Name]
                                                    ,   Size
                                                    ,   Extension
                                                    ,   CompressionType
                                                    ,   ContentType
                                                FROM [File]
                                                WHERE Id = @Id;
                                                """, connection);

                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                return LoadFile(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<MyFile>> GetFiles(IEnumerable<int> ids)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var criteriaIds = string.Join(',', ids.Select(id => id.ToString()));
                
                using var cmd = new SqlCommand($"""
                                                SELECT  Id
                                                    ,   CreatedDate
                                                    ,   [Name]
                                                    ,   Size
                                                    ,   Extension
                                                    ,   CompressionType
                                                    ,   ContentType
                                                FROM [File]
                                                WHERE Id IN ({criteriaIds});
                                                """, connection);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                return await LoadFiles(reader);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<MyFile>> GetFilesFromCampaign(int id)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                SELECT f.Id
                                                    , f.CreatedDate
                                                    , f.[Name]
                                                    , f.Size
                                                    , f.Extension
                                                    , f.CompressionType
                                                    , f.ContentType
                                                 FROM [File] AS f
                                                     INNER JOIN Campaign_File AS cf
                                                         ON cf.FileId = f.Id
                                                WHERE CampaignId = @CampaignId
                                                """, connection);

                cmd.Parameters.AddWithValue("@CampaignId", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                return await LoadFiles(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<MyFile>> GetFilesFromCollection(int id)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT f.Id" +
                                                ", f.CreatedDate" +
                                                ", f.[Name]" +
                                                ", f.Size" +
                                                ", f.Extension" +
                                                ", f.CompressionType" +
                                                ", f.ContentType" +
                                            " FROM [File] AS f" +
                                                " INNER JOIN Collection_File AS cf" +
                                                    " ON cf.FileId = f.Id" +
                                            $" WHERE CollectionId = @CollectionId", connection);

                cmd.Parameters.AddWithValue("@CollectionId", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                return await LoadFiles(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<MyFile>> GetFilesFromAuxiliary(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                SELECT f.Id
                                                    , f.CreatedDate
                                                    , f.[Name]
                                                    , f.Size
                                                    , f.Extension
                                                    , f.CompressionType
                                                    , f.ContentType
                                                 FROM [File] AS f
                                                     INNER JOIN Auxiliary_File AS cf
                                                         ON cf.FileId = f.Id
                                                WHERE AuxiliaryId = @AuxiliaryId
                                                """, connection);

                cmd.Parameters.AddWithValue("@AuxiliaryId", id);

                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                return await LoadFiles(reader);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        private static async Task<IEnumerable<MyFile>> LoadFiles(SqlDataReader reader)
        {
            var list = new List<MyFile>();
            while (await reader.ReadAsync().AsNoContext())
                list.Add(LoadFile(reader));

            await reader.CloseAsync().AsNoContext();

            return list;
        }

        private static MyFile LoadFile(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                Name = reader.GetString("Name"),
                Size = reader.GetInt64("Size"),
                Extension = reader.GetString("Extension"),
                CompressionType = reader.GetString("CompressionType"),
                ContentType = reader.GetString("ContentType")
            };

        public async Task<int> Insert(MyFile file)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand(   """
                                            INSERT INTO [File] ([Name], CreatedDate, Extension, Size, CompressionType)
                                            OUTPUT INSERTED.Id
                                            VALUES (@Name, @CreatedDate, @Extension, @Size, @CompressionType)
                                            """, connection);

                cmd.Parameters.AddWithValue("@Name", file.Name);
                cmd.Parameters.AddWithValue("@CreatedDate", file.CreatedDate);
                cmd.Parameters.AddWithValue("@Extension", file.Extension);
                cmd.Parameters.AddWithValue("@Size", file.Size);
                cmd.Parameters.AddWithValue("@CompressionType", file.CompressionType);

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<int>> InsertFilesToAuxiliary(int id, IEnumerable<MyFile> files)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(file);
                    
                    using (var cmd = new SqlCommand("""
                                                    INSERT INTO Auxiliary_File (AuxiliaryId, FileId)
                                                    VALUES (@AuxiliaryId, @FileId)
                                                    """, connection))
                    {
                        cmd.Parameters.AddWithValue("@AuxiliaryId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<int>> InsertFilesToCampaign(int id, IEnumerable<MyFile> files)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(file);

                    using (var cmd = new SqlCommand("""
                                                    INSERT INTO Campaign_File (CampaignId, FileId)
                                                    VALUES (@CampaignId, @FileId)
                                                    """, connection))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<IEnumerable<int>> InsertFilesToCollection(int id, IEnumerable<MyFile> files)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(file);

                    using (var cmd = new SqlCommand("""
                                                    INSERT INTO Collection_File (CollectionId, FileId)
                                                    VALUES (@CollectionId, @FileId)
                                                    """, connection))
                    {
                        cmd.Parameters.AddWithValue("@CollectionId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task SaveFile(int id, string compressionType, string contentType, string content)
        {
            try
            {
                connection.Open();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                UPDATE [File]
                                                    SET CompressionType = @CompressionType
                                                    ,   ContentType = @ContentType
                                                WHERE Id = @Id
                                                """, connection);

                cmd.Parameters.AddWithValue("@CompressionType", compressionType);
                cmd.Parameters.AddWithValue("@ContentType", contentType);
                cmd.Parameters.AddWithValue("@Id", id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);

                cmd.CommandText =   """
                                    INSERT INTO File_Content (FileId, Content)
                                    VALUES (@Id, @Content)
                                    """;

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Content", content);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<(string, string)> GetFileContent(int id)
        {
            try
            {
                connection.Open();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand( """
                                                SELECT f.ContentType, c.Content
                                                FROM [File] AS f
                                                    INNER JOIN File_Content AS c
                                                        ON c.FileId = f.Id
                                                WHERE f.Id = @Id
                                                """, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.Read())
                    throw new Exception("File not found.");

                var contentType = reader.GetString("ContentType");
                var content = reader.GetString("Content");

                return (contentType, content);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task DeleteFiles(IEnumerable<int> ids)
            => await DeleteFiles(ids, null);

        public async Task DeleteFile(int id, IDbTransaction? transaction = null)
            => await DeleteFiles([id], transaction);

        private async Task DeleteFiles(IEnumerable<int> ids, IDbTransaction? transaction = null)
        {
            if (!ids.Any())
                return;

            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync().AsNoContext();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var criteria = string.Join(',', ids);

                using var cmd = new SqlCommand( $"""
                                                DELETE FROM [File]
                                                WHERE Id IN ({criteria})
                                                """, connection, transaction as SqlTransaction);

                await cmd.ExecuteNonQueryAsync().AsNoContext();
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().AsNoContext();
            }
        }
    }
}
