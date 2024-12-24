using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.API;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    public class FranchiseePurchaseSuggestionStoreViewModel : BaseViewModel
    {
        private readonly TemporaryLocalRepository _cache;
        private bool _loaded;
        private int? _purchaseId;

        private Store? _store;
        private bool _saveIsEnabled;

        public FieldViewModelCollection Fields { get; }

        public Visibility VisibilityComboBoxStore => _store == null ? Visibility.Visible : Visibility.Hidden;
        public Visibility VisibilityTextBlockStore => _store == null ? Visibility.Hidden : Visibility.Visible;
        public Visibility VisibilityFamilies => TotalSuggestedItems.HasValue && TotalSuggestedItems > 0 ? Visibility.Visible : Visibility.Hidden;
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
                    LoadContent().ConfigureAwait(false);
                }
            }
        }
        public Collection Collection { get; private set; }
        public PurchaseStatus? Status { get; private set; }
        public FieldViewModel<decimal?> Target { get; private set; }
        public FieldViewModel<decimal?> AverageTicket { get; private set; }
        public FieldViewModel<int?> PartsPerService { get; private set; }
        public FieldViewModel<decimal?> Coverage { get; private set; }
        public int? TotalSuggestedItems { get; private set; }
        public decimal SumPercentageFamilies { get; private set; }
        public decimal SumPercentageFamiliesView => SumPercentageFamilies * 100;
        public int SumItemsFamilies { get; private set; }
        public PurchaseSuggestionFamilyViewModel[] Families { get; private set; }

        public Visibility VisibilityButtonSave { get; private set; }
        public bool SaveIsEnabled
        {
            get => _saveIsEnabled;
            private set { _saveIsEnabled = value; OnPropertyChanged(); }
        }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand GoToNextFieldCommand { get; }
        public RelayCommand GoToPreviusFieldCommand { get; }
        public RelayCommand SaveCommand { get; }

        public FranchiseePurchaseSuggestionStoreViewModel()
        {
            _cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];

            Fields = new FieldViewModelCollection();
            Fields.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == "Value")
                    SaveIsEnabled = true;
            };

            Families = Array.Empty<PurchaseSuggestionFamilyViewModel>();
            
            Status = null;
            VisibilityButtonSave = Visibility.Hidden;

            Target = new FieldViewModel<decimal?>() { Tag = this };
            PartsPerService = new FieldViewModel<int?>() { Tag = this };
            AverageTicket = new FieldViewModel<decimal?>() { Tag = this };
            Coverage = new FieldViewModel<decimal?>() { Tag = this };

            Target.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                    UpdateTotalSuggestedItems();
            };
            PartsPerService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                    UpdateTotalSuggestedItems();
            };
            AverageTicket.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                    UpdateTotalSuggestedItems();
            };
            Coverage.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                    UpdateTotalSuggestedItems();
            };

            Fields.Add(Target);
            Fields.Add(PartsPerService);
            Fields.Add(AverageTicket);

            LoadedCommand = new RelayCommand(LoadStore);
            GoToNextFieldCommand = new RelayCommand(Fields.GoToNext);
            GoToPreviusFieldCommand = new RelayCommand(Fields.GoToPrevius);
            SaveCommand = new RelayCommand(async () => await Save());
        }

        private async Task Save()
        {
            try
            {
                DesableContent();

                if (!_purchaseId.HasValue)
                {
                    var purchase = new Purchase()
                    {
                        CollectionId = Collection.Id,
                        Items = Array.Empty<PurchaseItem>(),
                        Status = PurchaseStatus.Opened,
                        StoreId = Store.Id
                    };

                    _purchaseId = await ApiPurchase.Save(purchase);
                    if (!_purchaseId.HasValue)
                        throw new Exception("Falha ao salvar pedido de compra.");
                }

                var purchaseSuggestion = new PurchaseSuggestion
                {
                    PurchaseId = _purchaseId.Value,
                    AverageTicket = AverageTicket.Value,
                    Coverage = Coverage.Value,
                    PartsPerService = PartsPerService.Value,
                    Target = Target.Value,
                    TotalSuggestedItems = TotalSuggestedItems,
                    Families = Families.Select(fvm => new PurchaseSuggestionFamily()
                                                    {
                                                        FamilyId = fvm.Family.Id,
                                                        FamilySuggestedItems = fvm.FamilySuggestedItems,
                                                        Percentage = fvm.Percentage.Value,
                                                        Sizes = fvm.Sizes.Select(svm => svm.Item)
                                                                         .ToArray()
                                                    })
                                        .ToArray()
                };

                var id = await API.ApiPurchaseSuggestion.Save(purchaseSuggestion);

                await LoadPurchaseSuggestion();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void LoadStore()
        {
            if (_loaded)
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
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar informações da loja", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _loaded = true;
                EnableContent();
            }
        }

        private async Task LoadContent()
        {
            try
            {
                DesableContent();

                if (_store != null)
                {
                    Legendable?.SendMessage("Obtendo informações de compra...");
                    Collection = await API.ApiCollection.GetOpened();
                    OnPropertyChanged(nameof(Collection));

                    if (Collection == null)
                    {
                        MessageBox.Show(Me, "Não existe um período de compra aberto.", "BROTHERS - Fora do período de compras", MessageBoxButton.OK, MessageBoxImage.Error);
                        Navigator.ReturnNavigation();
                        return;
                    }

                    Legendable?.SendMessage("Carregando informações salvas...");
                    var purchase = await API.ApiPurchase.Get(Collection.Id, _store.Id);

                    _purchaseId = purchase?.Id;

                    SaveIsEnabled = false;

                    if (purchase == null ||
                        purchase.Status == PurchaseStatus.Opened)
                        SetStatusOpened();
                    else
                        SetStatusClosed();

                    await LoadPurchaseSuggestion();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                Legendable?.SendMessage(string.Empty);
            }
        }

        private void SetStatusClosed()
        {
            VisibilityButtonSave = Visibility.Hidden;
            Status = PurchaseStatus.Closed;
            PurchaseItemViewModel.ReadyOnly = true;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(Status));
        }

        private void SetStatusOpened()
        {
            VisibilityButtonSave = Visibility.Visible;
            Status = PurchaseStatus.Opened;
            PurchaseItemViewModel.ReadyOnly = false;

            OnPropertyChanged(nameof(VisibilityButtonSave));
            OnPropertyChanged(nameof(Status));
        }

        public override bool BeforeReturn()
        {
            if (SaveIsEnabled)
                return MessageBox.Show(Me, "Existem alterações que não foram salvas, deseja continuar?", "BROTHERS - Deseja sair sem salvar?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            return true;
        }

        public async Task LoadPurchaseSuggestion()
        {
            try
            {
                DesableContent();

                if (!_purchaseId.HasValue)
                    return;

                var families = await _cache.LoadFamilies();

                Legendable?.SendMessage("Carregando informações cadastradas...");
                var purchaseSuggestion = await API.ApiPurchaseSuggestion.GetByPurchaseId(_purchaseId.Value);

                PurchaseSuggestionFamilySizeViewModel.ReadyOnly = false;

                AverageTicket.Value = purchaseSuggestion?.AverageTicket;
                Coverage.Value = purchaseSuggestion?.Coverage ?? (decimal)2.5;
                Target.Value = purchaseSuggestion?.Target;
                PartsPerService.Value = purchaseSuggestion?.PartsPerService;
                TotalSuggestedItems = purchaseSuggestion?.TotalSuggestedItems;

                Fields.RemoveAll(f => f.Tag != this);

                var list = new List<PurchaseSuggestionFamilyViewModel>();
                foreach (var f in families)
                {
                    if (f.Name == "Camiseta" || f.Name == "Camisa" || f.Name == "Polo" || f.Name == "Calça" || f.Name == "Bermuda")
                    {
                        var purchaseSuggestionFamily = purchaseSuggestion?.Families?.FirstOrDefault(i => i.FamilyId == f.Id);
                        if (purchaseSuggestionFamily != null)
                            purchaseSuggestionFamily.Family = f;

                        var vm = purchaseSuggestionFamily == null ?
                                new PurchaseSuggestionFamilyViewModel(f) :
                                new PurchaseSuggestionFamilyViewModel(purchaseSuggestionFamily);
                        vm.ViewModel = this;
                        list.Add(vm);
                    }
                }

                Families = list.ToArray();

                OnPropertyChanged(nameof(Families));
                OnPropertyChanged(nameof(VisibilityFamilies));
                UpdateSumFamilies();

                SaveIsEnabled = false;

                Fields.GoToFirst();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar informações para a compra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
                Legendable?.SendMessage(string.Empty);
            }
        }

        public void UpdateSumFamilies()
        {
            SumPercentageFamilies = 0;
            SumItemsFamilies = 0;
            foreach (var f in Families)
            {
                SumPercentageFamilies += f.Percentage.Value;
                SumItemsFamilies += f.FamilySuggestedItems;
            }

            IFieldViewModel? firstSizeField = null;
            foreach (var f in Families)
            {
                if (SumPercentageFamilies == 1)
                {
                    f.LoadSizes();
                    firstSizeField ??= f.Sizes.First().Percentage;
                }
                else
                    f.UnloadSizes();
                
                f.UpdateFamilySuggestedItems();
            }

            OnPropertyChanged(nameof(SumItemsFamilies));
            OnPropertyChanged(nameof(SumPercentageFamilies));
            OnPropertyChanged(nameof(SumPercentageFamiliesView));

            if (firstSizeField != null)
                Fields.SetFocus(firstSizeField);
        }

        public void UpdateTotalSuggestedItems()
        {
            var averageTicket = AverageTicket.Value ?? 0;

            if (averageTicket == 0)
                TotalSuggestedItems = 0;
            else
            {
                var target = Target.Value ?? 0;
                var partsPerService = PartsPerService.Value ?? 0;
                var coverage = Coverage.Value ?? 0;
                TotalSuggestedItems = (int)Math.Round(target / averageTicket * partsPerService * coverage, 0);
            }

            OnPropertyChanged(nameof(TotalSuggestedItems));
            OnPropertyChanged(nameof(VisibilityFamilies));
        }
    }
}
