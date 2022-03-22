using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class FileRepository
    {
        private static object _lockerContent = new object();

        public static async Task<MyFile> GetFile(SqlConnection connection, int id)
        {
            bool connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT Id" +
                                                ", CreatedDate" +
                                                ", [Name]" +
                                                ", Size" +
                                                ", Extension" +
                                                ", CompressionType" +
                                            " FROM [File]" +
                                            " WHERE Id = @Id", connection);

                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                return LoadFile(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<MyFile[]> GetFiles(SqlConnection connection, int[] ids)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var criteriaIds = string.Join(',', ids.Select(id => id.ToString()));
                
                using var cmd = new SqlCommand("SELECT Id" +
                                                ", CreatedDate" +
                                                ", [Name]" +
                                                ", Size" +
                                                ", Extension" +
                                                ", CompressionType" +
                                            " FROM [File]" +
                                            $" WHERE Id IN ({criteriaIds})", connection);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                return await LoadFiles(reader);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<MyFile[]> GetFilesFromCampaign(SqlConnection connection, int id)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT f.Id" +
                                                ", f.CreatedDate" +
                                                ", f.[Name]" +
                                                ", f.Size" +
                                                ", f.Extension" +
                                                ", f.CompressionType" +
                                            " FROM [File] AS f" +
                                                " INNER JOIN Campaign_File AS cf" +
                                                    " ON cf.FileId = f.Id" +
                                            $" WHERE CampaignId = @CampaignId", connection);

                cmd.Parameters.AddWithValue("@CampaignId", id);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                return await LoadFiles(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<MyFile[]> GetFilesFromCollection(SqlConnection connection, int id)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT f.Id" +
                                                ", f.CreatedDate" +
                                                ", f.[Name]" +
                                                ", f.Size" +
                                                ", f.Extension" +
                                                ", f.CompressionType" +
                                            " FROM [File] AS f" +
                                                " INNER JOIN Collection_File AS cf" +
                                                    " ON cf.FileId = f.Id" +
                                            $" WHERE CollectionId = @CollectionId", connection);

                cmd.Parameters.AddWithValue("@CollectionId", id);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                return await LoadFiles(reader);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<MyFile[]> GetFilesFromAuxiliary(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT f.Id" +
                                                ", f.CreatedDate" +
                                                ", f.[Name]" +
                                                ", f.Size" +
                                                ", f.Extension" +
                                                ", f.CompressionType" +
                                            " FROM [File] AS f" +
                                                " INNER JOIN Auxiliary_File AS cf" +
                                                    " ON cf.FileId = f.Id" +
                                            $" WHERE AuxiliaryId = @AuxiliaryId", connection);

                cmd.Parameters.AddWithValue("@AuxiliaryId", id);

                using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

                if (await reader.ReadAsync())
                    return await LoadFiles(reader);

                return Array.Empty<MyFile>();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        private static async Task<MyFile[]> LoadFiles(SqlDataReader reader)
        {
            var list = new List<MyFile>();
            while (await reader.ReadAsync())
                list.Add(LoadFile(reader));

            await reader.CloseAsync();

            return list.ToArray();
        }

        private static MyFile LoadFile(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                Name = reader.GetString("Name"),
                Size = reader.GetInt64("Size"),
                Extension = reader.GetString("Extension"),
                CompressionType = reader.GetString("CompressionType")
            };

        public static async Task<int> Insert(SqlConnection connection, MyFile file)
        {
            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("INSERT INTO [File] ([Name], CreatedDate, Extension, Size, CompressionType)" +
                                            " OUTPUT INSERTED.Id" +
                                            " VALUES (@Name, @CreatedDate, @Extension, @Size, @CompressionType)", connection);

                cmd.Parameters.AddWithValue("@Name", file.Name);
                cmd.Parameters.AddWithValue("@CreatedDate", file.CreatedDate);
                cmd.Parameters.AddWithValue("@Extension", file.Extension);
                cmd.Parameters.AddWithValue("@Size", file.Size);
                cmd.Parameters.AddWithValue("@CompressionType", file.CompressionType);

                var dbid = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int[]> InsertFilesToAuxiliary(SqlConnection connection, int id, MyFile[] files)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(connection, file);
                    
                    using (var cmd = new SqlCommand("INSERT INTO Auxiliary_File (AuxiliaryId, FileId)" +
                                                    " VALUES (@AuxiliaryId, @FileId)", connection))
                    {
                        cmd.Parameters.AddWithValue("@AuxiliaryId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int[]> InsertFilesToCampaign(SqlConnection connection, int id, MyFile[] files)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(connection, file);

                    using (var cmd = new SqlCommand("INSERT INTO Campaign_File (CampaignId, FileId)" +
                                                    " VALUES (@CampaignId, @FileId)", connection))
                    {
                        cmd.Parameters.AddWithValue("@CampaignId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<int[]> InsertFilesToCollection(SqlConnection connection, int id, MyFile[] files)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var listIds = new List<int>();
                foreach (var file in files)
                {
                    var dbId = await Insert(connection, file);

                    using (var cmd = new SqlCommand("INSERT INTO Collection_File (CollectionId, FileId)" +
                                                    " VALUES (@CollectionId, @FileId)", connection))
                    {
                        cmd.Parameters.AddWithValue("@CollectionId", id);
                        cmd.Parameters.AddWithValue("@FileId", dbId);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    listIds.Add(dbId);
                }

                return listIds.ToArray();
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static void SaveFile(SqlConnection connection, int id, string compressionType, string contentType, string content)
        {
            lock (_lockerContent)
            {
                try
                {
                    connection.Open();

                    if (connection.State != ConnectionState.Open)
                        throw new Exception(MessageRepositories.ConnectionNotOpenException);

                    using var cmd = new SqlCommand("UPDATE [File]" +
                                                " SET CompressionType = @CompressionType" +
                                                    ", ContentType = @ContentType" +
                                                " WHERE Id = @Id", connection);

                    cmd.Parameters.AddWithValue("@CompressionType", compressionType);
                    cmd.Parameters.AddWithValue("@ContentType", contentType);
                    cmd.Parameters.AddWithValue("@Id", id);

                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);

                    cmd.CommandText = "INSERT INTO File_Content (FileId, Content) VALUES (@Id, @Content)";

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Content", content);

                    if (cmd.ExecuteNonQuery() == 0)
                        throw new Exception(MessageRepositories.UpdateFailException);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public static (string, string) GetFileContent(SqlConnection connection, int id)
        {
            lock (_lockerContent)
            {
                try
                {
                    connection.Open();

                    if (connection.State != ConnectionState.Open)
                        throw new Exception(MessageRepositories.ConnectionNotOpenException);

                    using var cmd = new SqlCommand("SELECT f.ContentType, c.Content" +
                                                    " FROM [File] AS f" +
                                                        " INNER JOIN File_Content AS c" +
                                                            " ON c.FileId = f.Id" +
                                                    " WHERE f.Id = @Id", connection);

                    cmd.Parameters.AddWithValue("@Id", id);

                    using var reader = cmd.ExecuteReader();

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
        }

        public static async Task DeleteFile(SqlConnection connection, int id, IDbTransaction? transaction = null)
            => await DeleteFiles(connection, new int[] { id }, transaction);

        public static async Task DeleteFiles(SqlConnection connection, int[] ids, IDbTransaction? transaction = null)
        {
            if (ids.Length == 0)
                return;

            var connectionWasOpened = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connectionWasOpened = false;
                    await connection.OpenAsync();
                }

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var criteria = string.Join(',', ids);

                using var cmd = new SqlCommand("DELETE FROM [File]" +
                                            $" WHERE Id IN ({criteria})", connection, transaction as SqlTransaction);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                if (!connectionWasOpened)
                    await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
