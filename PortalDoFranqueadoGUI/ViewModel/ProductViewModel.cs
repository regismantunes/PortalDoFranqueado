using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Util;
using PortalDoFranqueado.View;
using System;
using System.Linq;

namespace PortalDoFranqueado.ViewModel
{
    public class ProductViewModel : BaseNotifyPropertyChanged, IExpandable
    {
        private bool _focused;

        public RelayCommand OpenFileViewCommand { get; }

        public INavigatorViewModel? Navigator { get; set; }
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

        public PurchaseSuggestionFamily? SuggestionFamily { get; set; }

        public ProductViewModel(Product product, PurchaseItem[]? items = null)
        {
            Product = product;

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
                                                                           i.Size.Size == s.Size)?
                                                      .Quantity
                                 })
                     })
                    .ToList()
                    .OrderBy(i => i.Value.Item.Size.Order)
                    .ToArray();

            Items.ToList()
                 .ForEach(item => item.PropertyChanged += (sender, e) => UpdateAmount());

            UpdateAmount();

            OpenFileViewCommand = new RelayCommand(OpenFileView);

        }

        private void UpdateAmount()
        {
            Amount = (Product?.Price ?? 0) * Items.Sum(item => item.Value.Quantity ?? 0);
            OnPropertyChanged(nameof(Amount));
        }

        private void OpenFileView()
        {
            if (Navigator == null ||
                FileView == null)
                return;

            Navigator.NavigateTo(new ViewImage(FileView));
        }
    }
}
