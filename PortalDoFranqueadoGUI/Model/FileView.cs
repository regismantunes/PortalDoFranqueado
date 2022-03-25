using PortalDoFranqueadoGUI.Util;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
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

                if (!string.IsNullOrEmpty(_filePath))
                {
                    var fileInfo = new FileInfo(_filePath);
                    FileExists = fileInfo.Exists &&
                                 fileInfo.Length == Size;
                }
                else
                    FileExists = false;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileExists)));
            }
        }

        public bool FileExists { get; private set; }
        public ImageSource? ImageData { get; private set; }
        public Stretch Stretch { get; private set; }

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
            CompressionType = myFile.CompressionType;
        }

        public bool IsImage { get => ContentType.Contains("image"); }

        public void PrepareDirectory(string? directoryToSave = null)
        {
            IsTempDirectory = string.IsNullOrEmpty(directoryToSave);

            var folderPath = IsTempDirectory ?
                Path.Combine(Path.GetTempPath(), "BROTHERS", "Franqueados") :
                directoryToSave;

            FilePath = Path.Combine(folderPath, string.Concat(Id, '_', Name, Extension));
        }

        public async Task Download(string? directoryToSave = null)
        {
            if (string.IsNullOrEmpty(FilePath))
                PrepareDirectory(directoryToSave);

            if (!FileExists)
                await UpdateContentDataAsync();
        }

        private async Task UpdateContentDataAsync()
        {
            var tempFile = await API.ApiFile.DownloadFile(this);

            if (tempFile != null &&
                !string.IsNullOrEmpty(FilePath))
            {
                File.Move(tempFile, FilePath, true);
                FileExists = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileExists)));
            }
        }

        public void LoadImageData()
        {
            if (FilePath == null)
                throw new ArgumentNullException(nameof(FilePath));

            try
            {
                if (IsImage)
                {
                    var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

                    var imageData = new BitmapImage();
                    imageData.BeginInit();
                    imageData.StreamSource = fileStream;
                    imageData.EndInit();

                    ImageData = imageData;
                    Stretch = Stretch.Uniform;
                }
                else
                {
                    ImageData = IconManager.GetToImageSourceFromIcon(FilePath);
                    Stretch = Stretch.None;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageData)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Stretch)));
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
