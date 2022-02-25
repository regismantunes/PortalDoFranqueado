using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Repository;
using PortalDoFranqueadoGUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class PurchaseStoreViewModel : BaseViewModel, IReloadable
    {
        public bool ExpandGroups { get; set; } = true;
        private readonly LocalRepository _cache;
        private int _indexFocus;
        private FieldViewModel<PurchaseItemViewModel>[] _fields;
        private Store? _store;
        private decimal _amount;
        
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
        public ProductViewModel[] Products { get; set; }
        public PurchaseStatus? Status { get; private set; }
        public Visibility VisibilityButtonSave { get; private set; }
        public decimal Amount
        {
            get => _amount;
            private set { _amount = value; OnPropertyChanged(); }
        }

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
                else
                    OnPropertyChanged(nameof(Products));
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
                var propertyGroup = new PropertyGroupDescriptionPublicChange("FamilyName");
                
                if (_store != null)
                {
                    Collection = await API.ApiCollection.GetOpened();
                    OnPropertyChanged(nameof(Collection));

                    if (Collection == null)
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

                        var tabIndex = 0;
                        var productsVM = new List<ProductViewModel>();
                        foreach(var product in products.OrderBy(p => p.FileId)
                                                       .OrderBy(p => p.Price)
                                                       .OrderBy(p => p.Family?.Name))
                        {
                            var fileView = repository?.GetFile(product.FileId);
                            fileView?.StartDownload(repository.Drive);
                            var productVM = new ProductViewModel(product, purchase?.Items.Where(i => i.ProductId == product.Id)
                                                                                        .ToArray())
                            {
                                FileView = fileView
                            };
                            productVM.Items
                                .ToList()
                                .ForEach(item =>
                                {
                                    item.Value.PropertyChanged += (sender, args) =>
                                    {
                                        if (args.PropertyName == "Quantity")
                                        {
                                            propertyGroup.CallPropertyChange("GroupNames");
                                            UpdateAmount();
                                        }
                                    };
                                });
                            
                            productsVM.Add(productVM);
                        }

                        Products = productsVM.ToArray();

                        _fields = PurchaseItemViewModel.ItemsReadyOnly ?
                            Array.Empty<FieldViewModel<PurchaseItemViewModel>>() :
                            Products.SelectMany(p => p.Items)
                                    .Where(i => i.Value.IsEnabled)
                                    .ToArray();
                        
                        for (var i = 0; i < _fields.Length; i++)
                        {
                            _fields[i].TabIndex = i;
                            _fields[i].GotFocus += (sender, args) => _indexFocus = ((FieldViewModel<PurchaseItemViewModel>)sender).TabIndex;
                        }
                    }
                }

                if (emptyProducts)
                {
                    Products = Array.Empty<ProductViewModel>();
                    _fields = Array.Empty<FieldViewModel<PurchaseItemViewModel>>();
                }

                var view = (CollectionView)CollectionViewSource.GetDefaultView(Products);
                view.GroupDescriptions.Add(propertyGroup);

                OnPropertyChanged(nameof(Products));
                UpdateAmount();
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

        private void UpdateAmount()
        {
            Amount = Products
                .Select(productVM =>
                        new
                        {
                            Price = productVM?.Product.Price ?? 0m,
                            Quantity = productVM?.Items.Sum(item => item.Value.Quantity) ?? 0
                        })
                .Sum(item => item.Price * item.Quantity);
        }

        public void Reload() => LoadCollection();
    }
}
