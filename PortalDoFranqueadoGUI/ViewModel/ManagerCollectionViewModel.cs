using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class ManagerCollectionViewModel : BaseViewModel, IReloadable
    {
        public class ProductViewModel : BaseNotifyPropertyChanged
        {
            private bool _hasChange;
            private decimal? _price;
            private int? _familyId;
            private bool _focused;
            
            public int? Id { get; set; }
            public string FileId { get; set; }
            public FileView? FileView { get; set; }
            public decimal? Price 
            { 
                get => _price;
                set 
                { 
                    _price = value;
                    HasChange = true;
                    OnPropertyChanged();
                }
            }
            public int? FamilyId
            {
                get => _familyId;
                set
                {
                    _familyId = value;
                    HasChange = true;
                    OnPropertyChanged();
                }
            }
            public string? FamilyName { get; set; }
            public bool HasChange
            {
                get => _hasChange;
                set
                {
                    _hasChange = value;
                    OnPropertyChanged();
                }
            }
            public bool Focused
            {
                get => _focused;
                set
                {
                    _focused = value;
                    OnPropertyChanged();
                }
            }
            public bool CanDelete { get; set; }

            public ManagerCollectionViewModel ViewModel { get; set; }
        }

        private DateTime _collectionStartDate;
        private DateTime _collectionEndDate;
        private string _collectionFolderId;
        private readonly Collection _collection;
        private bool _itemsEnabled;

        public ObservableCollection<ProductViewModel> Products { get; }

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
        public string CollectionFolderId
        {
            get => _collectionFolderId;
            set
            {
                _collectionFolderId = value;
                OnPropertyChanged();
                UpdateCollection();
            }
        }
        public CollectionStatus CollectionStatus { get; set; }

        public bool ItemsEnabled
        {
            get => _itemsEnabled;
            private set
            {
                _itemsEnabled = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<ProductViewModel> SaveCommand { get; }
        public RelayCommand<ProductViewModel> DeleteCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public ManagerCollectionViewModel(Collection colecao)
        {
            CollectionStartDate = colecao.StartDate;
            CollectionEndDate = colecao.EndDate;
            CollectionFolderId = colecao.FolderId;
            _collection = colecao;

            Products = new ObservableCollection<ProductViewModel>();
            
            SaveCommand = new RelayCommand<ProductViewModel>(SaveProduct);
            DeleteCommand = new RelayCommand<ProductViewModel>(DeleteProduct);
            LoadedCommand = new RelayCommand(async () => await LoadProducts());
        }

        private async void UpdateCollection()
        {
            if (_collection is null)
                return;

            try
            {
                DesableContent();

                _collection.StartDate = CollectionStartDate;
                _collection.EndDate = CollectionEndDate;
                _collection.FolderId = CollectionFolderId;

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

        private async void SaveProduct(ProductViewModel productVM)
        {
            try
            {
                DesableContent();

                var product = new Product()
                { 
                    Id = productVM.Id,
                    Price = productVM.Price,
                    FileId = productVM.FileId,
                    FamilyId = productVM.FamilyId
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
            if (nextEmpty is not null)
                nextEmpty.Focused = true;
        }

        private async void DeleteProduct(ProductViewModel produto)
        {
            try
            {
                if (!produto.Id.HasValue)
                    return;

                DesableContent();

                await API.ApiProduct.Delete(produto.Id.Value);

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
            ProductViewModel? firstEmpty = null;
            try
            {
                DesableContent();
                ItemsEnabled = false;

                var products = await API.ApiProduct.Get(_collection.Id);

                var repository = API.Configuration.Current.Session.FilesRepository;
                var files = repository.GetFilesOnFolder(CollectionFolderId);
                foreach (var file in files)
                    file.StartDownload(repository.Drive);

                var cache = (LocalRepository)App.Current.Resources["Cache"];
                var families = await cache.LoadFamilies();

                Products.Clear();
                var listTmp = new List<ProductViewModel>();
                foreach (var product in products)
                {
                    var fileView = files.FirstOrDefault(file => file.Id == product.FileId);
                    if (fileView is not null)
                        files.Remove(fileView);

                    listTmp.Add(new ProductViewModel
                    {
                        Id = product.Id,
                        FamilyId = product.FamilyId,
                        FamilyName = product.FamilyId.HasValue ? families.First(f => f.Id == product.FamilyId).Name : string.Empty,
                        FileId = product.FileId,
                        Price = product.Price,
                        FileView = fileView,
                        CanDelete = fileView is null,
                        ViewModel = this,
                        HasChange = false
                    });
                }

                foreach (var file in files)
                {
                    var productVM = new ProductViewModel
                    {
                        FileId = file.Id,
                        FileView = file,
                        FamilyId = 0,
                        FamilyName = string.Empty,
                        CanDelete = false,
                        ViewModel = this,
                        HasChange = false
                    };

                    listTmp.Add(productVM);
                }

                var ordered = listTmp.OrderBy(p => p.FileId)
                                     .OrderBy(p => p.FamilyName)
                                     .OrderBy(p => p.FileView is null ? 0 : 1);

                foreach (var product in ordered)
                {
                    Products.Add(product);

                    if (firstEmpty is null)
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

                if (firstEmpty is not null)
                    firstEmpty.Focused = true;

                EnableContent();
            }
        }

        public async void Reload() => await LoadProducts();
    }
}
