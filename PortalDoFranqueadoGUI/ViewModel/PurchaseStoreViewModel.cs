using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Repository;
using System;
using System.Collections.Generic;
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
                    /*var list = new List<FieldViewModel<CompraItem>>();
                     FieldViewModel<CompraItem>? previusField = null;
                     foreach (var tamanho in Produto.Familia.Tamanhos)
                     {
                         var newField = new FieldViewModel<CompraItem>
                         {
                             Value = new CompraItem { Tamanho = tamanho },
                             PreviusField = previusField
                         };

                         if (previusField != null)
                             previusField.NextField = newField;

                         list.Add(newField);

                         previusField = newField;
                     }

                     Itens = list.OrderBy(i => i.Value.GetValueToOrder())
                                 .ToArray();*/

                    Items = (from s in Product.Family.Sizes
                            select new FieldViewModel<PurchaseItem>
                            {
                                Value = new PurchaseItem
                                {
                                    ProductId = Product.Id.Value,
                                    Product = Product,
                                    Size = s,
                                    Quantity = items?.FirstOrDefault(i => i.ProductId == Product.Id &&
                                                                          i.Size == s)?
                                                                          .Quantity
                                },
                            })
                            .OrderBy(i => i.Value.GetValueToOrder())
                            .ToArray();
                }
            }



            public Product Product { get; set; }
            public FileView? FileView { get; set; }
            public FieldViewModel<PurchaseItem>[] Items { get; }
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

        private LocalRepository _cache;
        private int _indexFocus;
        private FieldViewModel<PurchaseItem>[] _fields;
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
        public PurchaseStatus PurchaseStatus { get; private set; }
        public Visibility VisibilityButtonSave { get; private set; }
        public bool ItemsReadyOnly { get; private set; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand GoToNextFieldCommand { get; }
        public RelayCommand GoToPreviusFieldCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAndCloseCommand { get; }

        public PurchaseStoreViewModel()
        {
            _cache = (LocalRepository)App.Current.Resources["Cache"];

            _indexFocus = 0;
            _fields = Array.Empty<FieldViewModel<PurchaseItem>>();

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
                                   .Select(f => f.Value)
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
            PurchaseStatus = PurchaseStatus.Closed;
            ItemsReadyOnly = true;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(PurchaseStatus));
            OnPropertyChanged(nameof(ItemsReadyOnly));
        }

        private void EnableSave()
        {
            VisibilityButtonSave = Visibility.Visible;
            PurchaseStatus = PurchaseStatus.Opened;
            ItemsReadyOnly = false;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(PurchaseStatus));
            OnPropertyChanged(nameof(ItemsReadyOnly));
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

                        var ordered = products.OrderBy(p => p.FileId)
                                             .OrderBy(p => p.Price)
                                             .OrderBy(p => p.Family?.Name);

                        var repository = API.Configuration.Current.Session.FilesRepository;
                        Products = (from p in ordered
                                    select new ProductViewModel(p, 
                                                                purchase?.Items.Where(i => i.ProductId == p.Id)
                                                                               .ToArray())
                                    {
                                        FileView = repository?.GetFile(p.FileId),
                                    }).ToArray();

                        var listFields = new List<FieldViewModel<PurchaseItem>>();
                        foreach (var product in Products)
                        {
                            listFields.AddRange(product.Items);
                            product.FileView?.StartDownload(repository.Drive);
                        }

                        _fields = listFields.ToArray();
                    }
                }

                if (emptyProducts)
                {
                    Products = Array.Empty<ProductViewModel>();
                    _fields = Array.Empty<FieldViewModel<PurchaseItem>>();
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
