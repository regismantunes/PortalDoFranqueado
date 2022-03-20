using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Model.Order;
using PortalDoFranqueadoGUI.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class ManagerCollectionViewModel : BaseViewModel, IReloadable
    {
        public class LockedSizeViewModel : BaseNotifyPropertyChanged
        {
            private bool _isLocked;

            public string Size { get; set; }
            public bool IsEnabled { get => !IsLocked; set { IsLocked = !value; } }
            public bool IsLocked 
            { 
                get => _isLocked; 
                set 
                { 
                    _isLocked = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(TextColor)); 
                } 
            }
            public SolidColorBrush TextColor => IsLocked ? Brushes.LightGray : Brushes.Black;

            public RelayCommand ChangeLockedCommand;

            public LockedSizeViewModel()
            {
                ChangeLockedCommand = new RelayCommand(() =>
                {
                    IsLocked = !IsLocked;
                });
            }
        }

        public class CollectionProductViewModel : BaseNotifyPropertyChanged
        {
            private bool _hasChange;
            private decimal? _price;
            private int? _familyId;
            private bool _focused;
            private readonly List<string> _lockedSizes;

            public CollectionProductViewModel(ManagerCollectionViewModel owner, FileView file)
            {
                _lockedSizes = new List<string>();

                FileId = file.Id;
                FileView = file;
                FamilyId = 0;
                FamilyName = string.Empty;
                ViewModel = owner;
                HasChange = false;
            }

            public CollectionProductViewModel(ManagerCollectionViewModel owner, FileView file, Product product)
            {
                _lockedSizes = new List<string>();
                if (product.LockedSizes != null)
                    _lockedSizes.AddRange(product.LockedSizes);

                Id = product.Id;
                FamilyId = product.FamilyId;
                FamilyName = product.Family?.Name;
                FileId = product.FileId;
                Price = product.Price;
                FileView = file;
                ViewModel = owner;
                HasChange = false;
            }

            private void LoadLockedSizes()
            {
                if ((FamilyId ?? 0) == 0)
                {
                    LockedSizes = Array.Empty<FieldViewModel<LockedSizeViewModel>>();
                    return;
                }

                var cache = (LocalRepository)App.Current.Resources["Cache"];
                var family = cache.Families.First(f => f.Id == FamilyId);

                LockedSizes = family.Sizes.OrderBy(size => OrderSize.GetValue(size))
                                          .Select(size =>
                {
                    var field = new FieldViewModel<LockedSizeViewModel>()
                    {
                        Value = new LockedSizeViewModel()
                        {
                            Size = size,
                            IsLocked = _lockedSizes.Contains(size)
                        }
                    };

                    field.Value.PropertyChanged += (s, e) =>
                    {
                        if (field.Value.IsLocked &&
                            !_lockedSizes.Contains(size))
                            _lockedSizes.Add(size);
                        else if (!field.Value.IsLocked &&
                            _lockedSizes.Contains(size))
                            _lockedSizes.Remove(size);

                        HasChange = true;
                    };

                    return field;
                }).ToArray();

                OnPropertyChanged(nameof(LockedSizes));
            }

            public int? Id { get; set; }
            public int FileId { get; set; }
            public FileView FileView { get; }
            public decimal? Price { get => _price; set { _price = value; HasChange = true; OnPropertyChanged(); } }
            public int? FamilyId { get => _familyId; 
                set 
                { 
                    _familyId = value; 
                    HasChange = true; 
                    OnPropertyChanged(); 
                    LoadLockedSizes(); 
                } 
            }
            public string? FamilyName { get; private set; }
            public FieldViewModel<LockedSizeViewModel>[] LockedSizes { get; private set; }
            public bool HasChange { get => _hasChange; set { _hasChange = value; OnPropertyChanged(); } }
            public bool Focused { get => _focused; set { _focused = value; OnPropertyChanged(); } }

            public ManagerCollectionViewModel ViewModel { get; set; }

            
        }

        private DateTime _collectionStartDate;
        private DateTime _collectionEndDate;
        private readonly Collection _collection;
        private bool _itemsEnabled;

        public ObservableCollection<CollectionProductViewModel> Products { get; }

        public DateTime CollectionStartDate
        {
            get => _collectionStartDate;
            set
            {
                _collectionStartDate = value;
                OnPropertyChanged();
                UpdateCollection();
            }
        }
        public DateTime CollectionEndDate
        {
            get => _collectionEndDate;
            set
            {
                _collectionEndDate = value;
                OnPropertyChanged();
                UpdateCollection();
            }
        }
        public CollectionStatus CollectionStatus { get; set; }

        public bool ItemsEnabled
        {
            get => _itemsEnabled && CanEdit;
            private set
            {
                _itemsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool CanEdit { get; }

        public RelayCommand<CollectionProductViewModel> SaveCommand { get; }
        public RelayCommand<CollectionProductViewModel> DeleteCommand { get; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand AddFilesCommand { get; }

        public ManagerCollectionViewModel(Collection colecao, bool canEdit)
        {
            CanEdit = canEdit;

            CollectionStartDate = colecao.StartDate;
            CollectionEndDate = colecao.EndDate;
            _collection = colecao;

            Products = new ObservableCollection<CollectionProductViewModel>();
            
            SaveCommand = new RelayCommand<CollectionProductViewModel>(SaveProduct);
            DeleteCommand = new RelayCommand<CollectionProductViewModel>(DeleteProduct);
            LoadedCommand = new RelayCommand(async () => await LoadProducts());
            AddFilesCommand = new RelayCommand(async () => await AddFiles());
        }

        private async Task AddFiles()
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
                    if (openFileDialog.FileNames.Length == 0)
                        return;

                    for (var i = 0; i < openFileDialog.FileNames.Length; i++)
                    {
                        Legendable?.SendMessage($"Carregando arquivo {i + 1} de {openFileDialog.FileNames.Length}...");
                        var selectedFile = openFileDialog.FileNames[i];
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

                        var id = await API.ApiFile.InsertCollectionFiles(_collection.Id, new MyFile[] { myFile });

                        myFile.Id = id[0];
                        var file = new FileView(myFile);
                        file.LoadImage(selectedFile);
                        
                        Products.Insert(0, new CollectionProductViewModel(this, file));
                        
                        var bytes = File.ReadAllBytes(selectedFile);
                        await API.ApiFile.UploadFile(myFile, bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar fotos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Legendable?.SendMessage(string.Empty);
                EnableContent();
            }
        }

        private async void UpdateCollection()
        {
            if (_collection == null)
                return;

            try
            {
                DesableContent();

                _collection.StartDate = CollectionStartDate;
                _collection.EndDate = CollectionEndDate;

                await API.ApiCollection.Update(_collection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao atualizar informações", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async void SaveProduct(CollectionProductViewModel productVM)
        {
            try
            {
                DesableContent();

                var product = new Product()
                { 
                    Id = productVM.Id,
                    Price = productVM.Price,
                    FileId = productVM.FileId,
                    FamilyId = productVM.FamilyId,
                    LockedSizes = productVM.LockedSizes
                                           .Where(ls => ls.Value.IsLocked)
                                           .Select(ls => ls.Value.Size)
                                           .ToArray()
                };

                if (productVM.Id.HasValue)
                    await API.ApiProduct.Update(product);
                else
                {
                    var id = await API.ApiProduct.Insert(_collection.Id, product);
                    productVM.Id = id;
                }

                productVM.HasChange = false;

                SetFocusNextEmpty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao salvar produto", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void SetFocusNextEmpty()
        {
            var nextEmpty = Products.FirstOrDefault(item => !item.Id.HasValue);
            if (nextEmpty != null)
                nextEmpty.Focused = true;
        }

        private async void DeleteProduct(CollectionProductViewModel produto)
        {
            try
            {
                DesableContent();

                if (produto.Id.HasValue)
                    await API.ApiProduct.Delete(produto.Id.Value);
                else if (produto.FileView.Id != 0)
                    await API.ApiFile.Delete(produto.FileView.Id);

                Products.Remove(produto);

                SetFocusNextEmpty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao excluir produto", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async Task LoadProducts()
        {
            CollectionProductViewModel? firstEmpty = null;
            try
            {
                DesableContent();
                ItemsEnabled = false;

                Legendable?.SendMessage("Carregando fotos...");
                var myFiles = await API.ApiFile.GetFromCollection(_collection.Id);
                
                var files = new List<FileView>();
                for(int i = 0; i < myFiles.Length; i++)
                {
                    Legendable?.SendMessage($"Carregando fotos {i + 1} de {myFiles.Length}...");
                    var fileView = new FileView(myFiles[i]);
                    await fileView.StartDownload();
                    files.Add(fileView);
                }
                
                Legendable?.SendMessage("Carregando produtos...");
                var products = await API.ApiProduct.Get(_collection.Id);

                Legendable?.SendMessage("Carregando famílias...");
                var cache = (LocalRepository)App.Current.Resources["Cache"];
                var families = await cache.LoadFamilies();

                Legendable?.SendMessage("Configurando itens...");
                products.ToList()
                        .ForEach(product => product.Family = product.FamilyId.HasValue ? 
                                                            families.First(f => f.Id == product.FamilyId) : 
                                                            null);

                Products.Clear();
                var listTmp = new List<CollectionProductViewModel>();
                if (products.Any())
                    listTmp.AddRange(products.Select(product => 
                    {
                        var fileView = files.FirstOrDefault(file => file.Id == product.FileId);

                        if (fileView != null)
                            files.Remove(fileView);

                        return new CollectionProductViewModel(this, fileView, product);
                    }));

                if (files.Any())
                    listTmp.AddRange(files.Select(file => new CollectionProductViewModel(this, file)));

                var ordered = listTmp.OrderBy(p => p.FileId)
                                     .OrderBy(p => p.FamilyName)
                                     .OrderBy(p => p.FileView == null ? 0 : 1);

                foreach (var product in ordered)
                {
                    Products.Add(product);

                    if (firstEmpty == null)
                        firstEmpty = product;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar produtos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ItemsEnabled = true;

                if (firstEmpty != null)
                    firstEmpty.Focused = true;

                Legendable?.SendMessage(string.Empty);
                EnableContent();
            }
        }

        public async void Reload() => await LoadProducts();
    }
}
