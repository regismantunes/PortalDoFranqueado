using PortalDoFranqueadoGUI.GoogleDrive;
using PortalDoFranqueadoGUI.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Repository
{
    public class FilesRepository
    {
        public GoogleDriveFilesRepository Drive { get; }

        public string FotosFolderId { get; set; }
        public string ApoioFolderId { get; set; }

        public FilesRepository(bool readOnly, string clientSecret, string driveServiceCredentials, string applicationName) 
            => Drive = new GoogleDriveFilesRepository(!readOnly, clientSecret, driveServiceCredentials, applicationName);

        public IList<FileView> GetFilesOnFotosFolder()
            => GetFilesOnFolder(FotosFolderId);

        public IList<FileView> GetFilesOnApoioFolder()
            => GetFilesOnFolder(ApoioFolderId);

        public IList<FileView> GetFilesOnFolder(string folderId)
        {
            var fileList = Drive.GetContainsInFolder(folderId);
            return (from file in fileList
                    select new FileView(file)).ToList();
        }

        public FileView? GetFile(string fileId)
        {
            var file = Drive.GetFile(fileId);
            return file is null ? null : new FileView(file);
        }
    }
}
