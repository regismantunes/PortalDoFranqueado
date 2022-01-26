using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class PurchaseStoreViewModel : BaseViewModel, IReloadable
    {
        public class ProductViewModel : BaseNotifyPropertyChanged
        {
            private bool _focused;

            public ProductViewModel(Product product, PurchaseItem[]? items = null)
            {
                Product = product;

                if (Product.Family != null)
                {
                    Items = (from s in Product.Family.Sizes
                            select new FieldViewModel<PurchaseItemViewModel>
                            {
                                Value = new PurchaseItemViewModel(
                                        new PurchaseItem
                                    {
                                        ProductId = Product.Id.Value,
                                        Product = Product,
                                        Size = s,
                                        Quantity = items?.FirstOrDefault(i => i.ProductId == Product.Id &&
                                                                              i.Size == s)?
                                                                              .Quantity
                                    })
                            })
                            .ToList()
                            .OrderBy(i => i.Value.Item.GetValueToOrder())
                            .ToArray();
                }
            }

            public Product Product { get; set; }
            public FileView? FileView { get; set; }
            public FieldViewModel<PurchaseItemViewModel>[] Items { get; }
            public bool Focused
            {
                get => _focused;
                set
                {
                    _focused = value;
                    OnPropertyChanged();
                }
            }

            public PurchaseStoreViewModel ViewModel { get; set; }
        }

        public class PurchaseItemViewModel : BaseNotifyPropertyChanged
        {
            public int ProductId { get => Item.ProductId; set => Item.ProductId = value; }
            public Product? Product { get => Item.Product; set => Item.Product = value; }
            public string Size { get => Item.Size; set => Item.Size = value; }
            public int? Quantity { get => Item.Quantity; set => Item.Quantity = value; }

            public Visibility VisibilityTextBlockQuantity => ItemsReadyOnly ? Visibility.Visible : Visibility.Collapsed;
            public Visibility VisibilityTextBoxQuantity => !ItemsReadyOnly ? Visibility.Visible : Visibility.Collapsed;
            public PurchaseItem Item { get; }

            public PurchaseItemViewModel(PurchaseItem item)
            {
                Item = item;
                StaticPropertyChanged += (o, e) =>
                {
                    OnPropertyChanged(nameof(VisibilityTextBlockQuantity));
                    OnPropertyChanged(nameof(VisibilityTextBoxQuantity));
                };
            }

            private static bool _itemsReadyOnly = false;
            private static event PropertyChangedEventHandler? StaticPropertyChanged;
            internal static bool ItemsReadyOnly 
            { 
                get => _itemsReadyOnly;
                set
                {
                    _itemsReadyOnly = value;
                    StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ItemsReadyOnly)));
                }
            }
        }

        private readonly LocalRepository _cache;
        private int _indexFocus;
        private FieldViewModel<PurchaseItemViewModel>[] _fields;
        private Store? _store;
        
        public Visibility VisibilityComboBoxStore => _store == null ? Visibility.Visible : Visibility.Hidden;
        public Visibility VisibilityTextBlockStore => _store == null ? Visibility.Hidden : Visibility.Visible;
        public Store? Store
        { 
            get => _store;
            set
            {
                if (_store != value)
                {
                    _store = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(VisibilityComboBoxStore));
                    OnPropertyChanged(nameof(VisibilityTextBlockStore));
                    LoadCollection();
                }
            }
        }
        public Collection Collection { get; private set; }
        public ProductViewModel[] Products { get; private set; }
        public PurchaseStatus? Status { get; private set; }
        public Visibility VisibilityButtonSave { get; private set; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand GoToNextFieldCommand { get; }
        public RelayCommand GoToPreviusFieldCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAndCloseCommand { get; }

        public PurchaseStoreViewModel()
        {
            _cache = (LocalRepository)App.Current.Resources["Cache"];

            _indexFocus = 0;
            _fields = Array.Empty<FieldViewModel<PurchaseItemViewModel>>();

            Status = null;
            VisibilityButtonSave = Visibility.Hidden;

            LoadedCommand = new RelayCommand(LoadStore);
            GoToNextFieldCommand = new RelayCommand(GoToNextField);
            GoToPreviusFieldCommand = new RelayCommand(GoToPreviusField);
            SaveCommand = new RelayCommand(async () => await Save(false));
            SaveAndCloseCommand = new RelayCommand(async () => await Save(true));
        }

        private async Task Save(bool close)
        {
            try
            {
                DesableContent();

                var purchase = new Purchase
                {
                    StoreId = _store.Id,
                    CollectionId = Collection.Id,
                    Status = close ? PurchaseStatus.Closed : PurchaseStatus.Opened,
                    Items = _fields.Where(item => item.Value.Quantity > 0)
                                   .Select(f => f.Value.Item)
                                   .ToArray()
                };

                await API.ApiPurchase.Save(purchase);

                if (close)
                    DisableSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void DisableSave()
        {
            VisibilityButtonSave = Visibility.Hidden;
            Status = PurchaseStatus.Closed;
            PurchaseItemViewModel.ItemsReadyOnly = true;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(Status));
        }

        private void EnableSave()
        {
            VisibilityButtonSave = Visibility.Visible;
            Status = PurchaseStatus.Opened;
            PurchaseItemViewModel.ItemsReadyOnly = false;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(Status));
        }

        private void GoToNextField()
        {
            if (_fields.Length == 0)
                return;

            _fields[_indexFocus].IsFocused = false;

            if (_indexFocus >= _fields.Length - 1)
                _indexFocus = 0;
            else
                _indexFocus++;

            _fields[_indexFocus].IsFocused = true;
        }

        private void GoToPreviusField()
        {
            if (_fields.Length == 0)
                return;

            _fields[_indexFocus].IsFocused = false;

            if (_indexFocus == 0)
                _indexFocus = _fields.Length - 1;
            else
                _indexFocus--;

            _fields[_indexFocus].IsFocused = true;
        }

        private void GoToFirstField()
        {
            if (_fields.Length == 0)
                return;

            _fields[_indexFocus].IsFocused = false;

            _indexFocus = 0;
            _fields[_indexFocus].IsFocused = true;
        }

        private void GoToLastField()
        {
            if (_fields.Length == 0)
                return;

            _fields[_indexFocus].IsFocused = false;

            _indexFocus = _fields.Length - 1;
            _fields[_indexFocus].IsFocused = true;
        }

        private void LoadStore()
        {
            if (_store != null)
                return;

            try
            {
                DesableContent();

                if (_cache.Stores.Count == 0)
                {
                    Navigator.PreviousNavigate();
                    return;
                }

                Store = _cache.Stores.Count == 1 ? 
                    _cache.Stores[0] : 
                    null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar informações da loja", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async void LoadCollection()
        {
            try
            {
                DesableContent();

                bool emptyProducts = true;

                if (_store != null)
                {
                    Collection = await API.ApiCollection.GetOpened();
                    OnPropertyChanged(nameof(Collection));

                    if (Collection is null)
                    {
                        MessageBox.Show("Não existe um período de compra aberto.", "BROTHERS - Fora do período de compras", MessageBoxButton.OK, MessageBoxImage.Error);
                        Navigator.PreviousNavigate();
                        return;
                    }

                    var products = (await API.ApiProduct.Get(Collection.Id)).ToList();

                    if (products.Count > 0)
                    {
                        emptyProducts = false;

                        var purchase = await API.ApiPurchase.Get(Collection.Id, _store.Id);

                        if (purchase == null || 
                            purchase.Status == PurchaseStatus.Opened)
                            EnableSave();
                        else
                            DisableSave();
                        
                        var families = await _cache.LoadFamilies();
                        products.ForEach(p => p.Family = families.FirstOrDefault(f => f.Id == p.FamilyId));

                        var repository = API.Configuration.Current.Session.FilesRepository;

                        var productsVM = new List<ProductViewModel>();
                        foreach(var product in products.OrderBy(p => p.FileId)
                                                       .OrderBy(p => p.Price)
                                                       .OrderBy(p => p.Family?.Name))
                        {
                            var fileView = repository?.GetFile(product.FileId);
                            fileView?.StartDownload(repository.Drive);
                            productsVM.Add(new ProductViewModel(product, purchase?.Items.Where(i => i.ProductId == product.Id)
                                                                                        .ToArray())
                            {
                                FileView = fileView
                            });
                        }

                        Products = productsVM.ToArray();

                        _fields = PurchaseItemViewModel.ItemsReadyOnly ?
                            Array.Empty<FieldViewModel<PurchaseItemViewModel>>() :
                            Products.SelectMany(p => p.Items).ToArray();
                    }
                }

                if (emptyProducts)
                {
                    Products = Array.Empty<ProductViewModel>();
                    _fields = Array.Empty<FieldViewModel<PurchaseItemViewModel>>();
                }

                OnPropertyChanged(nameof(Products));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                GoToFirstField();
            }
        }

        public void Reload() => LoadCollection();
    }
}
