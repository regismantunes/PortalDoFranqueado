using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PortalDoFranqueadoGUI.Model;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class ManagerFilesViewModel : BaseViewModel, IReloadable
    {
        private readonly FileOwner _ownerType;
        private readonly int _id;

        public string TitleMessage { get; private set; }
        public ObservableCollection<FileView> Files { get; }
        public Visibility VisibilityChangeButtons { get; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand AddFilesCommand { get; }
        public RelayCommand<IList> DeleteCommand { get; }
        public RelayCommand DeleteAllCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand<IList> SaveLocalCommand { get; }

        public ManagerFilesViewModel(FileOwner ownerType, int id, string title)
        {
            _ownerType = ownerType;
            _id = id;
            TitleMessage = title;
            Files = new ObservableCollection<FileView>();
            VisibilityChangeButtons = API.Configuration.Current.Session.User.Role == UserRole.Manager ? Visibility.Visible : Visibility.Collapsed;
            
            LoadedCommand = new RelayCommand(async() => await LoadFiles());
            AddFilesCommand = new RelayCommand(AddFiles);
            SaveCommand = new RelayCommand(async () => await Save());
            DeleteCommand = new RelayCommand<IList>(Delete);
            DeleteAllCommand = new RelayCommand(DeleteAll);
            SaveLocalCommand = new RelayCommand<IList>(SaveLocal);
        }

        private async Task LoadFiles()
        {
            try
            {
                DesableContent();

                Files.Clear();

                var myFiles = _ownerType == FileOwner.Auxiliary ?
                    await API.ApiFile.GetFromAuxiliary(_id) :
                    await API.ApiFile.GetFromCampaign(_id);
                
                myFiles.ToList().ForEach(f => Files.Add(new FileView(f)));

                //Files.AsParallel().ForAll(f => f.StartDownload());
                foreach (var file in Files)
                    await file.StartDownload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void SaveLocal(IList selectedItems)
        {
            if (selectedItems.Count == 0)
                return;

            try
            {
                DesableContent();

                var initialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (selectedItems.Count == 1)
                {
                    var fileView = (FileView?)selectedItems[0];
                    if (fileView == null)
                        return;

                    var sfd = new SaveFileDialog()
                    {
                        Filter = $"Arquivos {fileView.Extension[1..].ToUpper()}|*{fileView.Extension}",
                        InitialDirectory = initialFolder,
                        FileName = string.Concat(fileView.Name, fileView.Extension)
                    };

                    if (sfd.ShowDialog() ?? false)
                        File.Copy(fileView.FilePath, sfd.FileName, true);
                }
                else
                {
                    var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
                    {
                        ShowNewFolderButton = true,
                        SelectedPath = initialFolder,
                    };

                    if (dialog.ShowDialog(Me) ?? false)
                    {
                        var folder = dialog.SelectedPath;
                        
                        foreach (var item in selectedItems)
                        {
                            var fileView = (FileView)item;
                            File.Copy(fileView.FilePath, Path.Combine(folder, string.Concat(fileView.Name, fileView.Extension)), true);
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao baixar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void AddFiles()
        {
            try
            {
                DesableContent();

                var openFileDialog = new OpenFileDialog()
                {
                    Multiselect = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach(var selectedFile in openFileDialog.FileNames)
                    {
                        var fileInfo = new FileInfo(selectedFile);
                        var mimeType = MimeTypes.MimeTypeMap.GetMimeType(fileInfo.FullName);

                        var myFile = new MyFile()
                        {
                            Name = fileInfo.Name[..^fileInfo.Extension.Length],
                            Extension = fileInfo.Extension,
                            CreatedDate = fileInfo.CreationTime,
                            Size = fileInfo.Length,
                            ContentType = mimeType
                        };

                        var file = new FileView(myFile);
                        file.LoadImage(selectedFile);
                        Files.Add(file);
                    }

                    //Files.ToList().ForEach(f => f.LoadImageData());
                    /*Files.AsParallel()
                         .ForAll(f => f.LoadImageData());*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async Task Save()
        {
            try
            {
                DesableContent();

                var insertedFiles = Files.Where(f => f.Id == 0 && !f.Removed)
                                         .ToArray();
                if (insertedFiles.Any())
                {
                    Legendable?.SendMessage($"Preparando ambiente para salvar {insertedFiles.Length} arquivos...");
                    var ids = _ownerType == FileOwner.Auxiliary ?
                        await API.ApiFile.InsertAuxiliaryFiles(_id, insertedFiles) :
                        await API.ApiFile.InsertCampaignFiles(_id, insertedFiles);

                    for (int i = 0; i < ids.Length; i++)
                    {
                        var fileView = insertedFiles[i];
                        Legendable?.SendMessage($"Salvando arquivo {fileView.Name} ({i + 1} de {ids.Length})...");
                        var file = fileView.Clone();
                        file.Id = ids[i];
                        var bytes = File.ReadAllBytes(fileView.FilePath);
                        await API.ApiFile.UploadFile(file, bytes);

                        fileView.Id = file.Id;
                    }
                }

                var removedFiles = Files.Where(f => f.Removed);
                if (removedFiles.Any())
                {
                    Legendable?.SendMessage($"Excluindo {removedFiles.Count()} arquivos...");

                    var removedIds = removedFiles.Where(f => f.Id > 0)
                                                 .Select(f => f.Id)
                                                 .ToArray();
                    
                    await API.ApiFile.Delete(removedIds);

                    removedFiles.ToList()
                                .ForEach(f => Files.Remove(f));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                Legendable?.SendMessage(string.Empty);
            }
        }

        private void Delete(IList selectedItems)
        {
            try
            {
                DesableContent();

                foreach(var file in selectedItems)
                    ((FileView)file).Removed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void DeleteAll()
        {
            try
            {
                DesableContent();

                Files.ToList().ForEach(f => f.Removed = true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        public async void Reload() => await LoadFiles();
    }
}
