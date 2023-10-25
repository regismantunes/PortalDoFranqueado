using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using NuGet;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Repository;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerFilesViewModel : BaseViewModel, IReloadable
    {
        private readonly FileOwner _ownerType;
        private readonly int _id;

        public string TitleMessage { get; private set; }
        public ObservableCollection<FileView> Files { get; }
        public Visibility VisibilityChangeButtons { get; }
        public double MaxWidthListView { get; private set; }

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
            SaveCommand = new RelayCommand(Save);
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

                Legendable?.SendMessage("Carregando arquivos...");
                var myFiles = _ownerType == FileOwner.Auxiliary ?
                    await API.ApiFile.GetFromAuxiliary(_id) :
                    await API.ApiFile.GetFromCampaign(_id);

                if (myFiles.Length == 0)
                    Legendable?.SendMessage(string.Empty);
                else
                {
                    Files.AddRange(myFiles.Select(f => new FileView(f)));

                    try
                    {
                        _ = Task.Factory.StartNew(() =>
                            {
                                var i = 1;
                                var length = Files.Count;
                                var hasError = false;
                                Files.ToArray()
                                    .AsParallel()
                                    .ForAll(async file =>
                                    {
                                        try
                                        {
                                            file.PrepareDirectory();
                                            if (!file.FileExists)
                                            {
                                                /*file.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
                                                {
                                                    Me?.Dispatcher.BeginInvoke(() =>
                                                    {
                                                        if (e.PropertyName == nameof(file.FileExists))
                                                        {
                                                            if (i < length)
                                                                Legendable?.SendMessage($"Carregando arquivos {i++} de {length}...");
                                                            else
                                                                Legendable?.SendMessage(string.Empty);
                                                        }
                                                    });
                                                };*/
                                                await file.Download();
                                            }

                                            Me?.Dispatcher.BeginInvoke(() =>
                                            {
                                                if (file.FileExists)
                                                    file.LoadImageData();

                                                if (i < length)
                                                    Legendable?.SendMessage($"Carregando arquivos {i++} de {length}...");
                                                else
                                                    Legendable?.SendMessage(string.Empty);
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            if (!hasError)
                                            {
                                                hasError = true;
                                                Me?.Dispatcher.BeginInvoke(() => MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error));
                                            }
                                        }
                                    });
                            });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MaxWidthListView = Me.ActualWidth - 50;
                OnPropertyChanged(nameof(MaxWidthListView));
                Me.SizeChanged += Me_SizeChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void Me_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidthListView = e.NewSize.Width - 50;
            OnPropertyChanged(nameof(MaxWidthListView));
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
                    _ = Task.Factory.StartNew(() =>
                    {
                        var i = 1;
                        var length = openFileDialog.FileNames.Length;
                        Legendable?.SendMessage($"Carregando {length} arquivos selecionados...");
                        foreach (var selectedFile in openFileDialog.FileNames)
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

                            Me?.Dispatcher.BeginInvoke(() =>
                            {
                                Files.Add(file);
                                file.LoadImage(selectedFile);

                                Legendable?.SendMessage(i < length ? $"Arquivo carregando ({i++} de {length})" : string.Empty);
                            });
                        }
                    });
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

        private bool _saving = false;

        private void Save()
        {
            if (_saving)
                return;

            try
            {
                DesableContent();

                Worker.StartWork(async () =>
                {
                    try
                    {
                        _saving = true;

                        var insertedFiles = Files.Where(f => f.Id == 0 && !f.Removed)
                                                 .ToArray();
                        if (insertedFiles.Any())
                        {
                            Legendable?.SendMessage($"Preparando ambiente para salvar {insertedFiles.Length} arquivos...");
                            var ids = _ownerType == FileOwner.Auxiliary ?
                                await API.ApiFile.InsertAuxiliaryFiles(_id, insertedFiles) :
                                await API.ApiFile.InsertCampaignFiles(_id, insertedFiles);

                            for (var i = 0; i < ids.Length; i++)
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
                            Me?.Dispatcher.BeginInvoke(() => removedFiles.Where(f => f.Id == 0)
                                                                         .ToList()
                                                                         .ForEach(f => Files.Remove(f)));

                            var removedIds = removedFiles.Where(f => f.Id > 0)
                                                         .ToArray();

                            for (var i = 0; i < removedIds.Length; i++)
                            {
                                var removedFile = removedIds[i];
                                Legendable?.SendMessage($"Excluindo arquivos ({i + 1} de {removedFiles.Count()})...");
                                await API.ApiFile.Delete(removedFile.Id);
                                Me?.Dispatcher.BeginInvoke(() => Files.Remove(removedFile));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Me?.Dispatcher.BeginInvoke(() => MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar fotos e vídeos", MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                    finally
                    {
                        Legendable?.SendMessage(string.Empty);
                        _saving = false;
                    }
                });
            }
            finally
            {
                EnableContent();
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

        public override bool BeforeReturn()
        {
            if (Files.Any(f => f.Id == 0 || 
                               f.Removed))
                return MessageBox.Show(Me, "Existem alterações que não foram salvas, deseja continuar?", "BROTHERS - Deseja sair sem salvar?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            return true;
        }
    }
}