using PortalDoFranqueadoAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Repositories
{
    public static class FileRepository
    {
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
                Extension = reader.GetString("Extension")
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

                var cmd = new SqlCommand("INSERT INTO [File] ([Name], CreatedDate, Extension, Size)" +
                                            " OUTPUT INSERTED.Id" +
                                            " VALUES (@Name, @CreatedDate, @Extension, @Size)", connection);

                cmd.Parameters.AddWithValue("@Name", file.Name);
                cmd.Parameters.AddWithValue("@CreatedDate", file.CreatedDate);
                cmd.Parameters.AddWithValue("@Extension", file.Extension);
                cmd.Parameters.AddWithValue("@Size", file.Size);

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

        public static async Task SaveFile(SqlConnection connection, int id, string contentType, string content)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("UPDATE [File]" +
                                            " SET ContentType = @ContentType" +
                                            ", Content = @Content" +
                                            " WHERE Id = @Id", connection);

                cmd.Parameters.AddWithValue("@ContentType", contentType);
                cmd.Parameters.AddWithValue("@Content", content);
                cmd.Parameters.AddWithValue("@Id", id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task<(string,string)> GetFileContent(SqlConnection connection, int id)
        {
            try
            {
                await connection.OpenAsync();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var cmd = new SqlCommand("SELECT ContentType, Content" +
                                                " FROM [File]" +
                                                " WHERE Id = @Id", connection);

                cmd.Parameters.AddWithValue("@Id", id);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    throw new Exception("File not found.");

                var contentType = reader.GetString("ContentType");
                var content = reader.GetString("Content");

                return (contentType, content);
            }
            finally
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        public static async Task DeleteFile(SqlConnection connection, int id)
            => await DeleteFiles(connection, new int[] { id });

        public static async Task DeleteFiles(SqlConnection connection, int[] ids)
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
                                            $" WHERE Id IN ({criteria})", connection);

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
