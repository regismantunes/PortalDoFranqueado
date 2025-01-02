using CommunityToolkit.Mvvm.Input;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.View;
using System;
using System.Linq;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerCollectionPurchasesViewModel : BaseViewModel
    {
        public class PurchaseViewModel : BaseNotifyPropertyChanged
        {
            private decimal _amount = decimal.MinValue;
            private Purchase _purchase;
            private bool _canReverse;

            public Purchase Purchase { get => _purchase; set { _purchase = value; OnPropertyChanged(); } }
            public PurchaseStatus Status { get => _purchase.Status; set { _purchase.Status = value; OnPropertyChanged();} }
            public decimal Amount 
            {
                get 
                { 
                    if (_amount == decimal.MinValue)
                        _amount = Purchase.Items.Sum(item => (item.Quantity ?? 0) * (item.Product?.Price ?? 0));

                    return _amount;
                }
            }

            public RelayCommand<Purchase> ViewCommand { get; set; }
            public RelayCommand<PurchaseViewModel> ReverseCommand { get; set; }
            public bool CanReverse { get => _canReverse; set { _canReverse = value; OnPropertyChanged(); } }
        }

        private readonly TemporaryLocalRepository _cache;

        public Collection Collection { get; }
        public PurchaseViewModel[] Purchases { get; private set; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand<Purchase> ViewPurchaseCommand { get; }
        public RelayCommand<PurchaseViewModel> ReversePurchaseCommand { get; }

        public ManagerCollectionPurchasesViewModel(Collection collection)
        {
            Collection = collection;

            _cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];

            LoadedCommand = new RelayCommand(LoadPurchases);
            ViewPurchaseCommand = new RelayCommand<Purchase>(ViewPurchase);
            ReversePurchaseCommand = new RelayCommand<PurchaseViewModel>(ReversePurchase);
        }
        
        public void ViewPurchase(Purchase purchase)
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerCollectionPurchase(purchase));
            }
            finally
            {
                EnableContent();
            }
        }

        public async void ReversePurchase(PurchaseViewModel purchaseVM)
        {
            try
            {
                DesableContent();

                await Api.ApiPurchase.Reverse(purchaseVM.Purchase.Id.Value);

                purchaseVM.Status = PurchaseStatus.Opened;
                purchaseVM.CanReverse = false;
            }
            finally
            {
                EnableContent();
            }
        }

        public async void LoadPurchases()
        {
            try
            {
                DesableContent();

                var purchases = await Api.ApiPurchase.GetPurchases(Collection.Id);

                if (purchases.Length > 0)
                {
                    if (_cache.Stores.Count == 0)
                        _cache.Stores = await Api.ApiStore.GetStores();
                    
                    var stores = _cache.Stores.ToArray();
                    var products = await Api.ApiProduct.Get(Collection.Id);

                    purchases.ToList()
                             .ForEach(purchase =>
                    {
                        purchase.Store = stores.FirstOrDefault(store => store.Id == purchase.StoreId);
                        purchase.Items.ToList()
                                      .ForEach(item => item.Product = products
                                      .FirstOrDefault(product => product.Id == item.ProductId)
                        );
                    });

                    Purchases = purchases.OrderBy(purchase => purchase.Store.Name)
                                         .Select(purchase => new PurchaseViewModel 
                                         {
                                            Purchase = purchase,
                                            ViewCommand = ViewPurchaseCommand,
                                            ReverseCommand = ReversePurchaseCommand,
                                            CanReverse = purchase.Status != PurchaseStatus.Opened && 
                                                         Collection.Status == CollectionStatus.Opened
                                         })
                                         .ToArray();
                }
                else
                    Purchases = Array.Empty<PurchaseViewModel>();

                OnPropertyChanged(nameof(Purchases));
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
