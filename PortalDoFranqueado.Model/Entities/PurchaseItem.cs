namespace PortalDoFranqueado.Model.Entities
{
    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public ProductSize Size { get; set; }
        public int? Quantity { get; set; }
    }
}
