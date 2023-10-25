using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PortalDoFranqueado.Export;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerCollectionPurchaseViewModel : FileViewViewModel
    {
        private readonly TemporaryLocalRepository _cache;

        private bool _loaded;

        public Purchase Purchase { get; private set; }
        public Store Store { get; private set; }
        public ProductViewModel[] Products { get; private set; }
        public decimal Amount { get; private set; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand ExportToExcelCommand { get; }

        public ManagerCollectionPurchaseViewModel(Purchase purchase)
        {
            Purchase = purchase;

            _cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];

            LoadedCommand = new RelayCommand(async () => await LoadPurchase());
            ExportToExcelCommand = new RelayCommand(async () => await ExportToExcel());
        }

        private async Task LoadPurchase(bool reload = false)
        {
            if (!reload && _loaded)
                return;

            Task? taskAfterLoad = null;

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

                var files = myFiles.Select(f => new FileView(f)).ToList();

                var filesToLoadImageData = files.ToArray();
                taskAfterLoad = new Task(() =>
                {
                    Task.Delay(500).Wait();
                    LoadFiles(filesToLoadImageData).ConfigureAwait(false);
                });

                Legendable?.SendMessage("Carregando familias e fornecedores...");
                var families = await _cache.LoadFamilies();
                var suppliers = await _cache.LoadSuppliers();
                products.ForEach(p =>
                    {
                        p.Family = families.FirstOrDefault(f => f.Id == p.FamilyId);
                        p.Supplier = suppliers.FirstOrDefault(s => s.Id == p.SupplierId);
                    });

                Legendable?.SendMessage("Configurando itens...");
                var productsVM = new List<ProductViewModel>();
                foreach (var product in products.OrderBy(p => p.FileId)
                                                .OrderBy(p => p.Price)
                                                .OrderBy(p => p.Family?.Name))
                {
                    var fileView = files.First(f => f.Id == product.FileId);
                    product.ImageInformation = new ImageInfo()
                    {
                        FileAddress = fileView.FilePath,
                        Width = fileView.ImageData?.Width,
                        Height = fileView.ImageData?.Height
                    };
                    fileView.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(fileView.ImageData))
                        {
                            product.ImageInformation.FileAddress = fileView.FilePath;
                            product.ImageInformation.Width = fileView.ImageData?.Width;
                            product.ImageInformation.Height = fileView.ImageData?.Height;
                        }
                    };
                    productsVM.Add(new ProductViewModel(product, Purchase.Items.Where(i => i.ProductId == product.Id)
                                                                               .ToArray())
                    {
                        FileView = fileView,
                        Navigator = Navigator
                    });
                }

                Products = productsVM.ToArray();
                Amount = productsVM.Sum(p => p.Amount);

                var view = (CollectionView)CollectionViewSource.GetDefaultView(Products);
                view.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));

                OnPropertyChanged(nameof(Products));
                OnPropertyChanged(nameof(Amount));
                taskAfterLoad?.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _loaded = true;
                EnableContent();
                Legendable?.SendMessage(string.Empty);
            }
        }

        private async Task ExportToExcel()
        {
            try
            {
                var sfd = new SaveFileDialog()
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    FileName = string.Concat(Store.Name, ".xlsx"),
                    Filter = "Pasta de Trabalho do Excel (*.xlsx)|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    OverwritePrompt = true,
                    Title = "Gerar arquivo Excel com informações de compra",
                    ValidateNames = true
                };

                if (sfd.ShowDialog() ?? false)
                {
                    var fullAddress = sfd.FileName;

                    DesableContent();

                    var purchase = new Purchase()
                    {
                        CollectionId = Purchase.CollectionId,
                        Id = Purchase.Id,
                        Status = Purchase.Status,
                        StoreId = Purchase.StoreId,
                        Store = Purchase.Store,
                        Items = Purchase.Items
                    };

                    purchase.Items.ToList()
                                  .ForEach(item => item.Product = Products?.FirstOrDefault(p => p.Product.Id == item.ProductId)?.Product);

                    await Exporter.ExportToExcel(purchase, fullAddress, Legendable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao gerar arquivo excel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
