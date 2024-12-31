using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PortalDoFranqueado.Util.Converter
{
    public class PurchaseItemGroupDescriptionConverter : IValueConverter
    {
        private class SizeSuggestion
        {
            public ProductSize Size { get; set; }
            public int TotalSelected { get; set; }
            public int TotalSuggested { get; set; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is CollectionViewGroup group)
            {
                var count = 0;
                var totalQuantity = 0;
                var totalValue = 0m;
                PurchaseSuggestionFamily? suggestion = null;
                var sizeSuggestions = new List<SizeSuggestion>();
                foreach (var productObj in group.Items)
                {
                    var product = (ProductViewModel)productObj;
                    suggestion ??= product.SuggestionFamily;
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
                                if (suggestion != null)
                                {
                                    var sizeSuggestion = sizeSuggestions.FirstOrDefault(s => s.Size.Size == item.Value.Item.Size.Size) ??
                                                                        new SizeSuggestion();
                                    sizeSuggestion.TotalSelected += quantity;
                                    if (sizeSuggestion.Size == null)
                                    {
                                        sizeSuggestion.Size = item.Value.Item.Size;
                                        sizeSuggestion.TotalSuggested = suggestion.Sizes.FirstOrDefault(s => s.Size.Size == item.Value.Item.Size.Size)?.SizeSuggestedItems ?? 0;
                                        sizeSuggestions.Add(sizeSuggestion);
                                    }
                                }
                            });
                    }
                }

                var descSuggestion = suggestion == null ? string.Empty : $"/{suggestion.FamilySuggestedItems:D}";
                var descSuggestionSizes = sizeSuggestions.OrderBy(s => s.Size.Order)
                                                         .Aggregate(string.Empty, (a, s) => $"{a} | {s.Size.Size} {s.TotalSelected:D}/{s.TotalSuggested:D}");

                return $" ({totalValue:C} | {count}/{group.ItemCount} itens | {totalQuantity:D}{descSuggestion} peças{descSuggestionSizes})";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
