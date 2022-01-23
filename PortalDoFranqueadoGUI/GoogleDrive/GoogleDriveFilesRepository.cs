using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PortalDoFranqueadoGUI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.GoogleDrive
{
    public class GoogleDriveFilesRepository
    {
        //defined scope.
        private static readonly string[] ScopesReadyonly = { Google.Apis.Drive.v3.DriveService.Scope.DriveReadonly };
        private static readonly string[] ScopesUpload = { Google.Apis.Drive.v3.DriveService.Scope.Drive };

        private readonly string[] _scopes;
        private readonly string _applicationName;
        private readonly UserCredential _userCredential;

        public GoogleDriveFilesRepository(bool fullScope, string clientSecret, string driveServiceCredentials, string applicationName)
        {
            _scopes = fullScope ? ScopesUpload : ScopesReadyonly;

            string appTempFolder = Path.Combine(Path.GetTempPath(), "BROTHERS", "Franqueados");

            DirectoryExtensions.CreateDirectoryChain(appTempFolder);

            var clientSecretFile = Path.Combine(appTempFolder, "client_secret.json");
            File.WriteAllText(clientSecretFile, clientSecret);

            var driveServiceCredentialsFile = Path.Combine(appTempFolder, "DriveServiceCredentials.json");
            if (!Directory.Exists(driveServiceCredentialsFile))
                Directory.CreateDirectory(driveServiceCredentialsFile);
            File.WriteAllText(Path.Combine(driveServiceCredentialsFile, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"), driveServiceCredentials);

            _applicationName = applicationName;

            using var stream = new FileStream(clientSecretFile, FileMode.Open, FileAccess.Read);
            _userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(driveServiceCredentialsFile, true)).Result;
        }

        //create Drive API service.
        private Google.Apis.Drive.v3.DriveService GetService_v3()
            => new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _userCredential,
                ApplicationName = _applicationName
            });

        public Google.Apis.Drive.v2.DriveService GetService_v2()
            => new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _userCredential,
                ApplicationName = _applicationName
            });

        public List<GoogleDriveFile> GetContainsInFolder(string folderId)
        {
            var childList = new List<string>();
            var serviceV2 = GetService_v2();
            var childrenIDsRequest = serviceV2.Children.List(folderId);
            do
            {
                var children = childrenIDsRequest.Execute();

                if (children.Items != null && 
                    children.Items.Count > 0)
                {
                    foreach (var file in children.Items)
                        childList.Add(file.Id);
                }
                childrenIDsRequest.PageToken = children.NextPageToken;

            } while (!string.IsNullOrEmpty(childrenIDsRequest.PageToken));

            //Get All File List
            var allFileList = GetDriveFiles();
            var filterFileList = allFileList.Where(x => childList.Contains(x.Id));

            return filterFileList.ToList();
        }

        //get all files from Google Drive.
        public List<GoogleDriveFile> GetDriveFiles()
        {
            var service = GetService_v3();

            // define parameters of request.
            var fileListRequest = service.Files.List();

            //listRequest.PageSize = 10;
            //listRequest.PageToken = 10;
            fileListRequest.Fields = "nextPageToken, files(createdTime, id, name, size, version, trashed, parents, mimeType, trashed)";

            //get file list.
            IList<Google.Apis.Drive.v3.Data.File> files = fileListRequest.Execute().Files;
            var fileList = new List<GoogleDriveFile>();

            if (files != null)
                foreach (var file in files)
                {
                    if (file.Trashed.HasValue &&
                        file.Trashed.Value)
                        continue;

                    var googleFile = new GoogleDriveFile
                    {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime,
                        Parents = file.Parents,
                        MimeType = file.MimeType
                    };
                    fileList.Add(googleFile);
                }

            return fileList;
        }

        //file Upload to the Google Drive.
        /*public void FileUpload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                DriveService service = GetService();
                
                string path = Path.Combine(HttpContext.Current.Server.MapPath("~/GoogleDriveFile"),
                Path.GetFileName(file.FileName));
                
                file.SaveAs(path);

                var FileMetaData = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(file.FileName),
                    MimeType = MimeMapping.GetMimeMapping(path)
                };

                FilesResource.CreateMediaUpload request;

                using var stream = new FileStream(path, FileMode.Open);
                request = service.Files.Create(FileMetaData, stream, FileMetaData.MimeType);
                request.Fields = "id";
                request.Upload();
            }
        }*/

        //Download file from Google Drive by fileId.
        public async Task<string> DownloadGoogleFile(string fileId, string? directoryToSave = null)
        {
            var service = GetService_v3();

            var folderPath = string.IsNullOrEmpty(directoryToSave) ? 
                Path.Combine(Path.GetTempPath(), "BROTHERS", "Franqueados") : 
                directoryToSave;

            var request = service.Files.Get(fileId);

            var file = request.Execute();

            var filePath = Path.Combine(folderPath, 
                string.IsNullOrEmpty(directoryToSave) ? 
                string.Concat(fileId,file.Name[file.Name.LastIndexOf('.')..]) : 
                file.Name);

            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                if (fileInfo.Exists)
                    fileInfo.Delete();

                var stream1 = new MemoryStream();

                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    if (progress.Status == DownloadStatus.Completed)
                        SaveStream(stream1, filePath);
                };

                await request.DownloadAsync(stream1);
            }

            return filePath;
        }

        public GoogleDriveFile? GetFile(string fileId)
        {
            var service = GetService_v3();

            var request = service.Files.Get(fileId);

            var file = request.Execute();

            if (file.Trashed.HasValue &&
                file.Trashed.Value)
                return null;

            return new GoogleDriveFile
            {
                Id = file.Id,
                Name = file.Name,
                Size = file.Size,
                Version = file.Version,
                CreatedTime = file.CreatedTime,
                Parents = file.Parents,
                MimeType = file.MimeType
            };
        }

        // file save to server path
        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using var file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);
            stream.WriteTo(file);
        }

        //Delete file from the Google drive
        public void DeleteFile(GoogleDriveFile files)
        {
            var service = GetService_v3();
            try
            {
                // Initial validation.
                if (service == null)
                    throw new ArgumentNullException("service");

                if (files == null)
                    throw new ArgumentNullException(nameof(files));

                // Make the request.
                service.Files.Delete(files.Id).Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.Delete failed.", ex);
            }
        }
    }
}
