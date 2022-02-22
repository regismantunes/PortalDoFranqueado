using PortalDoFranqueadoGUI.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PortalDoFranqueadoGUI.Util
{
    public class PurchaseItemGroupDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var group = value as CollectionViewGroup;

            var count = 0;
            var totalQuantity = 0;
            var totalValue = 0m;
            foreach (var productObj in group.Items)
            {
                var product = (ProductViewModel)productObj;
                var price = product.Product.Price ?? 0;
                var validItems = product.Items.Where(item => item.Value.Quantity > 0);
                if (validItems.Any())
                {
                    count++;
                    validItems
                        .ToList()
                        .ForEach(item =>
                        {
                            var quantity = item.Value.Quantity ?? 0;
                            totalQuantity += quantity;
                            totalValue += price * quantity;
                        });
                }
            }

            return $" ({count}/{group.ItemCount} itens / {totalQuantity:D} peças / {totalValue:C})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
