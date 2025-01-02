using PortalDoFranqueado.Model.Entities;
using System.ComponentModel;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    public class PurchaseSuggestionFamilySizeViewModel : BaseNotifyPropertyChanged
    {
        public decimal PercentageView
        { 
            get => Item.Percentage * 100;
            set 
            {
                Item.Percentage = value / 100;
                Percentage.Value = Item.Percentage;
                OnPropertyChanged();
            }
        }
        public FieldViewModel<decimal> Percentage { get; }
        public int SizeSuggestedItems 
        { 
            get => Item.SizeSuggestedItems ?? 0;
            set { Item.SizeSuggestedItems = value; OnPropertyChanged(); }
        }
        public ProductSize Size => Item.Size;

        public PurchaseSuggestionFamilySize Item { get; private set; }
        public Visibility VisibilityTextBlockPercent => ReadyOnly ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityTextBoxPercent => !ReadyOnly ? Visibility.Visible : Visibility.Collapsed;

        public PurchaseSuggestionFamilySizeViewModel(PurchaseSuggestionFamilySize purchaseSuggestionFamilySize)
        {
            Item = purchaseSuggestionFamilySize;
            Percentage = new FieldViewModel<decimal>(Item.Percentage) { Tag = this };
        }

        private static bool _readyOnly = false;
        private static event PropertyChangedEventHandler? StaticPropertyChanged;
        internal static bool ReadyOnly
        {
            get => _readyOnly;
            set
            {
                _readyOnly = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ReadyOnly)));
            }
        }
    }
}
