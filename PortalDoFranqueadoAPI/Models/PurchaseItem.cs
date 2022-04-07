namespace PortalDoFranqueadoAPI.Models
{
    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public ProductSize Size { get; set; }
        public int? Quantity { get; set; }
    }
}
