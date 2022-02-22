using PortalDoFranqueadoGUI.Model;
using System.Linq;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class ProductViewModel : BaseNotifyPropertyChanged
    {
        private bool _focused;

        public ProductViewModel(Product product, PurchaseItem[]? items = null)
        {
            Product = product;

            if (Product.Family != null)
            {
                Items = (from s in Product.Family.Sizes
                         select new FieldViewModel<PurchaseItemViewModel>
                         {
                             Value = new PurchaseItemViewModel(
                                     new PurchaseItem
                                     {
                                         ProductId = Product.Id.Value,
                                         Product = Product,
                                         Size = s,
                                         Quantity = items?.FirstOrDefault(i => i.ProductId == Product.Id &&
                                                                                  i.Size == s)?
                                                                           .Quantity
                                     })
                         })
                        .ToList()
                        .OrderBy(i => i.Value.Item.GetValueToOrder())
                        .ToArray();
            }
        }

        public bool IsExpanded { get; set; }
        public string FamilyName => Product.Family.Name;
        public Product Product { get; set; }
        public FileView? FileView { get; set; }
        public FieldViewModel<PurchaseItemViewModel>[] Items { get; }
        public bool Focused
        {
            get => _focused;
            set
            {
                _focused = value;
                OnPropertyChanged();
            }
        }

        public PurchaseStoreViewModel ViewModel { get; set; }
    }
}
