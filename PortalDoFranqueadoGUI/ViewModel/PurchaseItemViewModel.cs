using PortalDoFranqueadoGUI.Model;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class PurchaseItemViewModel : BaseNotifyPropertyChanged
    {
        public int ProductId { get => Item.ProductId; set => Item.ProductId = value; }
        public Product? Product { get => Item.Product; set => Item.Product = value; }
        public string Size { get => Item.Size; set => Item.Size = value; }
        public int? Quantity { get => Item.Quantity; set { Item.Quantity = value; OnPropertyChanged(); } }
        public bool IsEnabled { get => !Product?.LockedSizes?.Contains(Size) ?? true; }

        public Visibility VisibilityTextBlockQuantity => ItemsReadyOnly ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityTextBoxQuantity => !ItemsReadyOnly ? Visibility.Visible : Visibility.Collapsed;
        public PurchaseItem Item { get; }

        public PurchaseItemViewModel(PurchaseItem item)
        {
            Item = item;
            StaticPropertyChanged += (o, e) =>
            {
                OnPropertyChanged(nameof(VisibilityTextBlockQuantity));
                OnPropertyChanged(nameof(VisibilityTextBoxQuantity));
            };
        }

        private static bool _itemsReadyOnly = false;
        private static event PropertyChangedEventHandler? StaticPropertyChanged;
        internal static bool ItemsReadyOnly
        {
            get => _itemsReadyOnly;
            set
            {
                _itemsReadyOnly = value;
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ItemsReadyOnly)));
            }
        }
    }
}
