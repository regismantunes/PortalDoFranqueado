﻿using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PortalDoFranqueado.Export;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PortalDoFranqueado.ViewModel
{
    public class FranchiseePurchaseStoreViewModel : BaseViewModel, IReloadable
    {
        public bool ExpandGroups { get; set; } = true;
        private readonly LocalRepository _cache;
        private int _indexFocus;
        private FieldViewModel<PurchaseItemViewModel>[] _fields;
        private bool _loaded;
        private int? _purchaseId;

        private Store? _store;
        private decimal _amount;
        private bool _saveIsEnabled;
        private bool _exportIsEnabled;

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
        public bool SaveIsEnabled
        {
            get => _saveIsEnabled;
            private set { _saveIsEnabled = value; OnPropertyChanged(); }
        }
        public bool ExportIsEnabled
        {
            get => _exportIsEnabled;
            private set { _exportIsEnabled = value; OnPropertyChanged(); }
        }
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
        public RelayCommand ExportToExcelCommand { get; }

        public FranchiseePurchaseStoreViewModel()
        {
            _cache = (LocalRepository)App.Current.Resources["Cache"];

            _indexFocus = 0;
            _fields = Array.Empty<FieldViewModel<PurchaseItemViewModel>>();

            Status = null;
            VisibilityButtonSave = Visibility.Hidden;

            LoadedCommand = new RelayCommand(() => LoadStore());
            GoToNextFieldCommand = new RelayCommand(GoToNextField);
            GoToPreviusFieldCommand = new RelayCommand(GoToPreviusField);
            SaveCommand = new RelayCommand(async () => await Save(false));
            SaveAndCloseCommand = new RelayCommand(async () => await Save(true));
            ExportToExcelCommand = new RelayCommand(async () => await ExportToExcel());
        }

        private async Task ExportToExcel()
        {
            if (_purchaseId == null)
                return;

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

                    var purchase = await API.ApiPurchase.Get(_purchaseId.Value);

                    if(purchase == null)
                    {
                        MessageBox.Show(Me, "BROTHERS - Falha ao obter informações de compra", "Não foi possível recuperar as informações de compra para gerar o arquivo Excel.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    purchase.Store = Store;
                    purchase.Items.ToList()
                                  .ForEach(item => item.Product = Collection.Products?.FirstOrDefault(p => p.Id == item.ProductId));

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

                var id = await API.ApiPurchase.Save(purchase);

                _purchaseId = id;
                SaveIsEnabled = false;
                ExportIsEnabled = true;

                if (close)
                    SetStatusClosed();
                else
                    OnPropertyChanged(nameof(Products));
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void SetStatusClosed()
        {
            VisibilityButtonSave = Visibility.Hidden;
            Status = PurchaseStatus.Closed;
            PurchaseItemViewModel.ItemsReadyOnly = true;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(Status));
        }

        private void SetStatusOpened()
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

        private void LoadStore(bool reload = false)
        {
            if (!reload && _loaded)
                return;

            if (_store != null)
                return;

            try
            {
                DesableContent();

                if (_cache.Stores.Count == 0)
                {
                    Navigator.ReturnNavigation();
                    return;
                }

                Store = _cache.Stores.Count == 1 ? 
                    _cache.Stores[0] : 
                    null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar informações da loja", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _loaded = true;
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
                    Legendable?.SendMessage("Obtendo informações de compra...");
                    Collection = await API.ApiCollection.GetOpened();
                    OnPropertyChanged(nameof(Collection));

                    if (Collection == null)
                    {
                        MessageBox.Show(Me,"Não existe um período de compra aberto.", "BROTHERS - Fora do período de compras", MessageBoxButton.OK, MessageBoxImage.Error);
                        Navigator.ReturnNavigation();
                        return;
                    }

                    Legendable?.SendMessage("Carregando produtos...");
                    Collection.Products = await API.ApiProduct.Get(Collection.Id);
                    
                    if (Collection.Products.Length > 0)
                    {
                        emptyProducts = false;

                        Legendable?.SendMessage("Carregando informações salvas...");
                        var purchase = await API.ApiPurchase.Get(Collection.Id, _store.Id);

                        _purchaseId = purchase?.Id;

                        ExportIsEnabled = _purchaseId.HasValue;
                        SaveIsEnabled = false;

                        if (purchase == null || 
                            purchase.Status == PurchaseStatus.Opened)
                            SetStatusOpened();
                        else
                            SetStatusClosed();

                        Legendable?.SendMessage("Carregando fotos...");
                        var myFiles = await API.ApiFile.GetFromCollection(Collection.Id);

                        var files = new List<FileView>();
                        myFiles.ToList()
                               .ForEach(f => files.Add(new FileView(f)));

                        var filesArray = files.ToArray();

                        _ = Task.Factory.StartNew(async () =>
                        {
                            foreach(var fileView in filesArray)
                            {
                                await Task.Delay(100);
                                fileView.PrepareDirectory();
                                if (!fileView.FileExists)
                                    await fileView.Download();

                                Me.Dispatcher.Invoke(fileView.LoadImageData);
                            }
                        });

                        Legendable?.SendMessage("Carregando familias...");
                        var families = await _cache.LoadFamilies();
                        var products = Collection.Products.ToList();
                        products.ForEach(p => p.Family = families.FirstOrDefault(f => f.Id == p.FamilyId));

                        Legendable?.SendMessage("Configurando itens...");
                        var productsVM = new List<ProductViewModel>();
                        foreach(var product in products.OrderBy(p => p.FileId)
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
                            var productVM = new ProductViewModel(product, purchase?.Items.Where(i => i.ProductId == product.Id)
                                                                                        .ToArray())
                            {
                                FileView = fileView,
                                Navigator = Navigator
                            };
                            productVM.Items
                                .ToList()
                                .ForEach(item => item.Value.PropertyChanged += (sender, args) =>
                                                 {
                                                     if (args.PropertyName == nameof(item.Value.Quantity))
                                                     {
                                                         propertyGroup.CallPropertyChange("GroupNames");
                                                         Task.Factory.StartNew(() =>
                                                         {
                                                             SaveIsEnabled = true;
                                                             ExportIsEnabled = false;
                                                             UpdateAmount();
                                                         });
                                                     }
                                                 }
                                );
                            
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
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                GoToFirstField();
                Legendable?.SendMessage(string.Empty);
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
