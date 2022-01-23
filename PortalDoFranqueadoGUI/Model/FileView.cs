using PortalDoFranqueadoGUI.GoogleDrive;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace PortalDoFranqueadoGUI.Model
{
    public class FileView : GoogleDriveFile, INotifyPropertyChanged
    {
        public FileView()
        { }

        public FileView(GoogleDriveFile driveFile)
        {
            Id = driveFile.Id;
            Name = driveFile.Name;
            Parents = driveFile.Parents;
            Version = driveFile.Version;
            CreatedTime = driveFile.CreatedTime;
            Size = driveFile.Size;
            MimeType = driveFile.MimeType;
        }

        public void StartDownload(GoogleDriveFilesRepository repository, string? directoryToSave = null)
            => UpdateImageDataAsync(repository, directoryToSave);

        public string? FilePath { get; private set; }
        public BitmapImage? ImageData { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void UpdateImageDataAsync(GoogleDriveFilesRepository repository, string? directoryToSave = null)
        {
            FilePath = await repository.DownloadGoogleFile(Id, directoryToSave);

            if (string.IsNullOrEmpty(directoryToSave))
            {
                var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

                ImageData = new BitmapImage();
                ImageData.BeginInit();
                ImageData.StreamSource = fileStream;
                ImageData.EndInit();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageData)));
            }
        }
    }
}
