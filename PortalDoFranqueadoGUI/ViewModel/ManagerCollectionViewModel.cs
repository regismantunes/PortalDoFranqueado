using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerCollectionViewModel : FileViewViewModel, IReloadable
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
            private int? _supplierId;
            private bool _focused;
            private readonly List<string> _lockedSizes;
            private string? _description;

            public int? Id { get; set; }
            public int FileId { get; set; }
            public FileView FileView { get; }
            public decimal? Price { get => _price; set { _price = value; HasChange = true; OnPropertyChanged(); } }
            public int? FamilyId
            {
                get => _familyId;
                set
                {
                    _familyId = value;
                    HasChange = true;
                    LoadLockedSizes();
                    OnPropertyChanged();
                }
            }
            public string? FamilyName { get; private set; }
            public int? SupplierId
            {
                get => _supplierId;
                set
                {
                    _supplierId = value;
                    HasChange = true;
                    OnPropertyChanged();
                }
            }
            public string? SupplierName { get; private set; }
            public FieldViewModel<LockedSizeViewModel>[] LockedSizes { get; private set; }
            public bool HasChange { get => _hasChange; set { _hasChange = value; OnPropertyChanged(); } }
            public bool Focused { get => _focused; set { _focused = value; OnPropertyChanged(); } }
            public string? Description 
            { 
                get => _description;
                set 
                { 
                    _description = value;
                    HasChange = true;
                    OnPropertyChanged();
                }
            }

            public ManagerCollectionViewModel ViewModel { get; set; }
            public RelayCommand OpenFileViewCommand { get; }

            public CollectionProductViewModel(ManagerCollectionViewModel owner, FileView file)
            {
                _lockedSizes = new List<string>();

                FileId = file.Id;
                FileView = file;
                FamilyId = 0;
                FamilyName = string.Empty;
                SupplierId = 0;
                SupplierName = string.Empty;
                Description = string.Empty;
                ViewModel = owner;
                HasChange = false;

                OpenFileViewCommand = new RelayCommand(OpenFileView);
            }

            public CollectionProductViewModel(ManagerCollectionViewModel owner, FileView file, Product product)
            {
                _lockedSizes = new List<string>();
                if (product.LockedSizes != null)
                    _lockedSizes.AddRange(product.LockedSizes);

                Id = product.Id;
                FamilyId = product.FamilyId ?? 0;
                FamilyName = product.Family?.Name;
                SupplierId = product.SupplierId ?? 0;
                SupplierName = product.Supplier?.Name;
                Description = product.Description;
                FileId = product.FileId;
                Price = product.Price;
                FileView = file;
                ViewModel = owner;
                HasChange = false;

                OpenFileViewCommand = new RelayCommand(OpenFileView);
            }

            private void LoadLockedSizes()
            {
                if ((FamilyId ?? 0) == 0)
                {
                    LockedSizes = Array.Empty<FieldViewModel<LockedSizeViewModel>>();
                    return;
                }

                var cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];
                var family = cache.Families.First(f => f.Id == FamilyId);

                LockedSizes = family.Sizes.OrderBy(size => size.Order)
                                          .Select(size =>
                {
                    var field = new FieldViewModel<LockedSizeViewModel>()
                    {
                        Value = new LockedSizeViewModel()
                        {
                            Size = size.Size,
                            IsLocked = _lockedSizes.Contains(size.Size)
                        }
                    };

                    field.Value.PropertyChanged += (s, e) =>
                    {
                        if (field.Value.IsLocked &&
                            !_lockedSizes.Contains(size.Size))
                            _lockedSizes.Add(size.Size);
                        else if (!field.Value.IsLocked &&
                            _lockedSizes.Contains(size.Size))
                            _lockedSizes.Remove(size.Size);

                        HasChange = true;
                    };

                    return field;
                }).ToArray();

                OnPropertyChanged(nameof(LockedSizes));
            }

            private void OpenFileView()
            {
                if (ViewModel == null ||
                    FileView == null)
                    return;

                ViewModel.Navigator.NavigateTo(new ViewImage(FileView));
            }
        }

        private DateTime _collectionStartDate;
        private DateTime _collectionEndDate;
        private readonly Collection _collection;
        private bool _itemsEnabled;
        private bool _loaded;

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
            AddFilesCommand = new RelayCommand(AddFiles);
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
                    if (openFileDialog.FileNames.Length == 0)
                        return;

                    Worker.StartWork(async () =>
                    {
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

                            var id = await Api.ApiFile.InsertCollectionFiles(_collection.Id, new MyFile[] { myFile });

                            myFile.Id = id[0];
                            var file = new FileView(myFile);

                            Me?.Dispatcher.Invoke(() =>
                            {
                                file.LoadImage(selectedFile);
                                Products.Insert(0, new CollectionProductViewModel(this, file));
                            });

                            var bytes = File.ReadAllBytes(selectedFile);
                            await Api.ApiFile.UploadFile(myFile, bytes);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar fotos", MessageBoxButton.OK, MessageBoxImage.Error);
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

                await Api.ApiCollection.Update(_collection);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao atualizar informações", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                           .ToArray(),
                    SupplierId = productVM.SupplierId,
                    Description = productVM.Description
                };

                if (product.FamilyId == 0)
                    product.FamilyId = null;

                if (product.SupplierId == 0)
                    product.SupplierId = null;

                if (productVM.Id.HasValue)
                    await Api.ApiProduct.Update(product);
                else
                {
                    var id = await Api.ApiProduct.Insert(_collection.Id, product);
                    productVM.Id = id;
                }

                productVM.HasChange = false;

                SetFocusNextEmpty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao salvar produto", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    await Api.ApiProduct.Delete(produto.Id.Value);
                else if (produto.FileView.Id != 0)
                    await Api.ApiFile.Delete(produto.FileView.Id);

                Products.Remove(produto);

                SetFocusNextEmpty();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao excluir produto", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async Task LoadProducts(bool reload = false)
        {
            if (!reload && _loaded)
                return;

            CollectionProductViewModel? firstEmpty = null;
            Task? taskAfterLoad = null;

            try
            {
                DesableContent();
                ItemsEnabled = false;

                Legendable?.SendMessage("Preparando ambiente...");
                var myFiles = await Api.ApiFile.GetFromCollection(_collection.Id);

                var files = myFiles.Select(f => new FileView(f)).ToList();

                var filesToLoadImageData = files.ToArray();
                taskAfterLoad = new Task(() =>
                {
                    Task.Delay(500).Wait();
                    LoadFiles(filesToLoadImageData).ConfigureAwait(false);
                });

                Legendable?.SendMessage("Carregando produtos...");
                var products = await Api.ApiProduct.Get(_collection.Id);

                Legendable?.SendMessage("Carregando famílias...");
                var cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];
                var families = await cache.LoadFamilies();

                Legendable?.SendMessage("Carregando fornecedores...");
                var supplier = await cache.LoadSuppliers();

                Legendable?.SendMessage("Configurando itens...");
                products.ToList()
                        .ForEach(product =>
                        {
                            product.Family = product.FamilyId.HasValue ?
                                            families.First(f => f.Id == product.FamilyId) :
                                            null;
                            product.Supplier = product.SupplierId.HasValue ?
                                            supplier.First(s => s.Id == product.SupplierId) :
                                            null;
                        });

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

                    firstEmpty ??= product;
                }

                taskAfterLoad?.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar produtos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _loaded = true;
                ItemsEnabled = true;

                if (firstEmpty != null)
                    firstEmpty.Focused = true;

                Legendable?.SendMessage(string.Empty);
                EnableContent();
            }
        }

        public async void Reload() => await LoadProducts();

        public override bool BeforeReturn()
        {
            if (Products.Any(p => p.HasChange))
                return MessageBox.Show(Me, "Existem alterações que não foram salvas, deseja continuar?", "BROTHERS - Deseja sair sem salvar?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            return true;
        }
    }
}
