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
    internal class CollectionPurchaseViewModel : BaseViewModel
    {
        public class ProductViewModel
        {
            public ProductViewModel(Product product, PurchaseItem[]? items = null)
            {
                Product = product;

                if (Product.Family != null)
                {
                    Items = (from s in Product.Family.Sizes
                             select new PurchaseItem
                             {
                                 ProductId = Product.Id.Value,
                                 Product = Product,
                                 Size = s,
                                 Quantity = items?.FirstOrDefault(i => i.ProductId == Product.Id &&
                                                                       i.Size == s)?
                                                                               .Quantity
                             })
                            .ToList()
                            .OrderBy(i => i.GetValueToOrder())
                            .ToArray();
                }
            }

            public Product Product { get; set; }
            public FileView? FileView { get; set; }
            public PurchaseItem[] Items { get; }
        }

        private readonly LocalRepository _cache;
        private readonly int _purchaseId;

        public Purchase Purchase { get; private set; }
        public Store Store { get; }
        public ProductViewModel[] Products { get; private set; }

        public RelayCommand LoadedCommand { get; }

        public CollectionPurchaseViewModel(int purchaseId)
        {
            _purchaseId = purchaseId;
            _cache = (LocalRepository)App.Current.Resources["Cache"];

            LoadedCommand = new RelayCommand(LoadPurchase);
        }

        private async void LoadPurchase()
        {
            try
            {
                DesableContent();

                var purchase = await API.ApiPurchase.Get(_purchaseId);
                if (purchase == null)
                {
                    MessageBox.Show("A compra não foi encontrada.", "BROTHERS - Falha ao carregar compras", MessageBoxButton.OK, MessageBoxImage.Error);
                    Navigator.PreviousNavigate();
                    return;
                }

                Purchase = purchase;

                var products = (await API.ApiProduct.Get(purchase.CollectionId))
                                                    .Where(p => purchase.Items.Any(i => i.ProductId == p.Id))
                                                    .ToList();

                var families = await _cache.LoadFamilies();
                products.ForEach(p => p.Family = families.FirstOrDefault(f => f.Id == p.FamilyId));

                var repository = API.Configuration.Current.Session.FilesRepository;

                var productsVM = new List<ProductViewModel>();
                foreach (var product in products.OrderBy(p => p.FileId)
                                                .OrderBy(p => p.Price)
                                                .OrderBy(p => p.Family?.Name))
                {
                    var fileView = repository?.GetFile(product.FileId);
                    fileView?.StartDownload(repository.Drive);
                    productsVM.Add(new ProductViewModel(product, purchase.Items.Where(i => i.ProductId == product.Id)
                                                                               .ToArray())
                    {
                        FileView = fileView
                    });
                }

                Products = productsVM.ToArray();

                OnPropertyChanged(nameof(Products));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
