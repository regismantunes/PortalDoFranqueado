using PortalDoFranqueadoGUI.Model.Order;

namespace PortalDoFranqueadoGUI.Model
{
    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string Size { get; set; }
        public int? Quantity { get; set; }

        public int GetValueToOrder()
            => OrderSize.GetValue(Size);
    }
}
