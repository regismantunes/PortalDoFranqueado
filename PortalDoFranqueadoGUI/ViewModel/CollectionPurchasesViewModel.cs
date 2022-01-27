using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class CollectionPurchasesViewModel : BaseViewModel
    {
        public Collection Collection { get; }
        public Purchase[] Purchases { get; private set; }
        public RelayCommand LoadedCommand { get; }

        public CollectionPurchasesViewModel(Collection collection)
        {
            Collection = collection;

            LoadedCommand = new RelayCommand(LoadPurchases);
        }

        public async void LoadPurchases()
        {
            try
            {
                DesableContent();

                var purchases = await API.ApiPurchase.GetPurchases(Collection.Id);

                var products = await API.ApiProduct.Get(Collection.Id);

                purchases.ToList()
                         .ForEach(purchase =>
                {
                    var amount = (decimal)0;
                    purchase.Items.ToList()
                                  .ForEach(item =>
                    {
                        item.Product = products.FirstOrDefault(product => product.Id == item.ProductId);
                        if (item.Product != null &&
                            item.Quantity != null)
                            amount += (decimal)(item.Product.Price * item.Quantity);
                    });
                    purchase.Amount = amount;
                });

                Purchases = purchases.ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao carregar compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
