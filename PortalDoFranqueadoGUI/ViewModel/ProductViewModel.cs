﻿using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Util;
using System.Linq;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class ProductViewModel : BaseNotifyPropertyChanged, IExpandable
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

                Items.ToList()
                     .ForEach(item => item.PropertyChanged += (sender, e) => UpdateAmount());

                UpdateAmount();
            }
        }

        public bool IsExpanded { get; set; }
        public string FamilyName => Product.Family.Name;
        public Product Product { get; set; }
        public decimal Amount { get; private set; }
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

        private void UpdateAmount()
        {
            Amount = (Product?.Price ?? 0) * Items.Sum(item => item.Value.Quantity ?? 0);
            OnPropertyChanged(nameof(Amount));
        }
    }
}
