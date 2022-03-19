using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class ManagerCollectionPurchaseViewModel : BaseViewModel
    {
        private readonly LocalRepository _cache;

        public Purchase Purchase { get; private set; }
        public Store Store { get; private set; }
        public ProductViewModel[] Products { get; private set; }
        public decimal Amount { get; private set; }

        public RelayCommand LoadedCommand { get; }

        public ManagerCollectionPurchaseViewModel(Purchase purchase)
        {
            Purchase = purchase;

            _cache = (LocalRepository)App.Current.Resources["Cache"];

            LoadedCommand = new RelayCommand(LoadPurchase);
        }

        private async void LoadPurchase()
        {
            try
            {
                DesableContent();

                Store = _cache.Stores.First(store => store.Id == Purchase.StoreId);
                OnPropertyChanged(nameof(Store));

                Legendable?.SendMessage("Carregando produtos...");
                var products = (await API.ApiProduct.Get(Purchase.CollectionId))
                                                    .Where(p => Purchase.Items.Any(i => i.ProductId == p.Id))
                                                    .ToList();

                Legendable?.SendMessage("Carregando fotos...");
                var myFiles = await API.ApiFile.GetFromCollection(Purchase.CollectionId);

                var files = new List<FileView>();
                for (int i = 0; i < myFiles.Length; i++)
                {
                    Legendable?.SendMessage($"Carregando fotos {i + 1} de {myFiles.Length}...");
                    var fileView = new FileView(myFiles[i]);
                    await fileView.StartDownload();
                    files.Add(fileView);
                }

                Legendable?.SendMessage("Carregando famílias...");
                var families = await _cache.LoadFamilies();
                products.ForEach(p => p.Family = families.FirstOrDefault(f => f.Id == p.FamilyId));

                Legendable?.SendMessage("Configurando itens...");
                var productsVM = new List<ProductViewModel>();
                foreach (var product in products.OrderBy(p => p.FileId)
                                                .OrderBy(p => p.Price)
                                                .OrderBy(p => p.Family?.Name))
                {
                    var fileView = files.First(f => f.Id == product.FileId);
                    fileView?.StartDownload();
                    productsVM.Add(new ProductViewModel(product, Purchase.Items.Where(i => i.ProductId == product.Id)
                                                                               .ToArray())
                    {
                        FileView = fileView
                    });
                }

                Products = productsVM.ToArray();
                Amount = productsVM.Sum(p => p.Amount);

                var view = (CollectionView)CollectionViewSource.GetDefaultView(Products);
                view.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));

                OnPropertyChanged(nameof(Products));
                OnPropertyChanged(nameof(Amount));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                Legendable?.SendMessage(string.Empty);
            }
        }
    }
}
