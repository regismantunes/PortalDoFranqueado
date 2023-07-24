using NuGet;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    public class PurchaseSuggestionFamilyViewModel : BaseNotifyPropertyChanged, IExpandable
    {
        private bool _focused;
        private readonly IList<PurchaseSuggestionFamilySize> _sizes;

        public INavigatorViewModel? Navigator { get; set; }
        public bool IsExpanded { get; set; }
        public Family Family { get; private set; }
        public decimal PercentageView
        {
            get => Percentage.Value * 100;
            set => Percentage.Value = value / 100;
        }
        public FieldViewModel<decimal> Percentage { get; }
        public decimal SumPercentSizes { get; private set; }
        public decimal SumPercentSizesView => SumPercentSizes * 100;
        public int FamilySuggestedItems { get; private set; }
        public int FamilySuggestedSelectedSizes { get; private set; }
        public PurchaseSuggestionFamilySizeViewModel[] Sizes { get; private set; }
        public Visibility VisibilitySizes { get; private set; }
        public bool Focused
        {
            get => _focused;
            set
            {
                _focused = value;
                OnPropertyChanged();
            }
        }

        private FranchiseePurchaseSuggestionStoreViewModel _viewModel;
        public FranchiseePurchaseSuggestionStoreViewModel ViewModel 
        { 
            get => _viewModel;
            set 
            {   
                _viewModel = value;
                _viewModel.Fields.Add(Percentage);

                UpdateFamilySuggestedItems();
            }
        }

        public PurchaseSuggestionFamilyViewModel(Family family)
            :this(new PurchaseSuggestionFamily()
            { 
                Family = family,
                FamilyId = family.Id,
                Percentage = 0,
                FamilySuggestedItems = 0
            })
        { }

        public PurchaseSuggestionFamilyViewModel(PurchaseSuggestionFamily purchaseSuggestionFamily)
        {
            Sizes = Array.Empty<PurchaseSuggestionFamilySizeViewModel>();
            VisibilitySizes = Visibility.Collapsed;
            Family = purchaseSuggestionFamily.Family;
            FamilySuggestedItems = purchaseSuggestionFamily.FamilySuggestedItems ?? 0;
            Percentage = new FieldViewModel<decimal>(purchaseSuggestionFamily.Percentage) { Tag = this };
            Percentage.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    FamilySuggestedItems = (int)Math.Round((ViewModel?.TotalSuggestedItems ?? 0) * Percentage.Value, 0);
                    OnPropertyChanged(nameof(PercentageView));
                    OnPropertyChanged(nameof(FamilySuggestedItems));
                    Sizes.ToList()
                         .ForEach(svm => UpdateSizeSuggestedItems(svm));
                    ViewModel?.UpdateSumFamilies();
                }
            };

            _sizes = new List<PurchaseSuggestionFamilySize>();
            if (purchaseSuggestionFamily.Sizes != null)
                _sizes.AddRange(purchaseSuggestionFamily.Sizes);
        }

        public void UnloadSizes()
        {
            Sizes.ToList()
                 .ForEach(s => _viewModel.Fields.Remove(s.Percentage));
            VisibilitySizes = Visibility.Collapsed;
            OnPropertyChanged(nameof(VisibilitySizes));
        }

        public void LoadSizes()
        {
            try
            {
                if (Sizes.Length == 0)
                {
                    _viewModel?.DesableContent();
                    _viewModel?.Legendable?.SendMessage("Carregando tamanhos...");

                    var list = new List<PurchaseSuggestionFamilySizeViewModel>();
                    if (Family != null)
                    {
                        foreach (var s in Family.Sizes.OrderBy(s => s.Order))
                        {
                            var item = _sizes.FirstOrDefault(i => i.Size.Size == s.Size);
                            if (item == null)
                            {
                                item = new PurchaseSuggestionFamilySize()
                                {
                                    Percentage = 0,
                                    Size = s,
                                    SizeSuggestedItems = 0
                                };
                                _sizes.Add(item);
                            }

                            var vm = new PurchaseSuggestionFamilySizeViewModel(item);
                            vm.PropertyChanged += (sender, e) =>
                            {
                                switch(e.PropertyName)
                                {
                                    case nameof(vm.PercentageView):
                                        UpdateSizeSuggestedItems(vm);
                                        break;
                                    case nameof(vm.SizeSuggestedItems):
                                        UpdateTotals();
                                        break;
                                }
                            };
                            list.Add(vm);
                            _viewModel?.Fields.Add(vm.Percentage);
                        }
                    }

                    Sizes = list.ToArray();
                    OnPropertyChanged(nameof(Sizes));

                    UpdateTotals();
                }

                VisibilitySizes = Visibility.Visible;
                OnPropertyChanged(nameof(VisibilitySizes));
            }
            finally
            {
                _viewModel?.EnableContent();
                _viewModel?.Legendable?.SendMessage(string.Empty);
            }
        }

        private void UpdateSizeSuggestedItems(PurchaseSuggestionFamilySizeViewModel vm)
        {
            vm.SizeSuggestedItems = (int)Math.Round(FamilySuggestedItems * vm.Item.Percentage, 0);
        }

        private void UpdateTotals()
        {
            SumPercentSizes = Sizes.Sum(i => i.Item.Percentage);
            FamilySuggestedSelectedSizes = Sizes.Sum(i => i.Item.SizeSuggestedItems ?? 0);
            OnPropertyChanged(nameof(SumPercentSizes));
            OnPropertyChanged(nameof(SumPercentSizesView));
            OnPropertyChanged(nameof(FamilySuggestedSelectedSizes));
        }

        public void UpdateFamilySuggestedItems()
        {
            FamilySuggestedItems = (int)Math.Round((_viewModel?.TotalSuggestedItems ?? 0) * Percentage.Value, 0);
            Sizes.ToList()
                .ForEach(s => UpdateSizeSuggestedItems(s));
            OnPropertyChanged(nameof(FamilySuggestedItems));
        }
    }
}
