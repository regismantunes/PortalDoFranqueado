﻿using GalaSoft.MvvmLight.CommandWpf;
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
    internal class ManagerCollectionPurchaseViewModel : BaseViewModel
    {
        private readonly LocalRepository _cache;

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

            _cache = (LocalRepository)App.Current.Resources["Cache"];

            LoadedCommand = new RelayCommand(async () => await LoadPurchase());
            ExportToExcelCommand = new RelayCommand(async () => await ExportToExcel());
        }

        private async Task LoadPurchase(bool reload = false)
        {
            if (!reload && _loaded)
                return;

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
                myFiles.ToList()
                       .ForEach(f => files.Add(new FileView(f)));

                var filesArray = files.ToArray();

                _ = Task.Factory.StartNew(async () =>
                {
                    foreach (var fileView in filesArray)
                    {
                        await Task.Delay(100);
                        fileView.PrepareDirectory();
                        if (!fileView.FileExists)
                            await fileView.Download();

                        Me.Dispatcher.Invoke(fileView.LoadImageData);
                    }
                });

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
