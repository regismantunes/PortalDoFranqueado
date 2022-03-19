using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PortalDoFranqueadoGUI.Model
{
    public class FileView : MyFile, INotifyPropertyChanged
    {
        private bool _removed = false;
        private bool _isTempDirectory = false;
        private string? _filePath;

        public bool Removed 
        { 
            get => _removed;
            set
            {
                _removed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Removed)));

                if(_removed)
                {
                    ImageData = null;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageData)));
                }
            }
        }

        public bool IsTempDirectory
        {
            get => _isTempDirectory;
            private set
            {
                _isTempDirectory = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTempDirectory)));
            }
        }

        public string? FilePath 
        { 
            get => _filePath;
            set 
            {
                _filePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
            }
        }
        public BitmapImage? ImageData { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public FileView()
        { }

        public FileView(MyFile myFile)
        {
            Id = myFile.Id;
            Name = myFile.Name;
            CreatedDate = myFile.CreatedDate;
            Extension = myFile.Extension;
            Size = myFile.Size;
            ContentType = myFile.ContentType;
        }

        public async Task StartDownload(string? directoryToSave = null)
        {
            IsTempDirectory = string.IsNullOrEmpty(directoryToSave);

            var folderPath = IsTempDirectory ?
                Path.Combine(Path.GetTempPath(), "BROTHERS", "Franqueados") :
                directoryToSave;

            FilePath = Path.Combine(folderPath, string.Concat(Id, '_', Name, Extension));

            var loadImageData = string.IsNullOrEmpty(directoryToSave);

            var fileInfo = new FileInfo(FilePath);
            if (!fileInfo.Exists ||
                fileInfo.Length != Size)
                await UpdateImageDataAsync(loadImageData);
            else if (loadImageData)
                LoadImageData();
        }

        private async Task UpdateImageDataAsync(bool loadImageData)
        {
            var tempFile = await API.ApiFile.DownloadFile(Id);

            if (tempFile != null &&
                !string.IsNullOrEmpty(FilePath))
            {
                File.Move(tempFile, FilePath, true);

                if (loadImageData)
                    LoadImageData();
            }
        }

        public void LoadImageData()
        {
            if (FilePath == null)
                throw new ArgumentNullException(nameof(FilePath));

            try
            {
                var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

                var imageData = new BitmapImage();
                imageData.BeginInit();
                imageData.StreamSource = fileStream;
                imageData.EndInit();

                ImageData = imageData;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageData)));
            }
            catch (NotSupportedException) { }
            catch
            {
                throw;
            }
        }

        public void LoadImage(string filePath)
        {
            FilePath = filePath;
            LoadImageData();
        }
    }
}
