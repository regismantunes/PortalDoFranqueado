namespace PortalDoFranqueadoAPI.Models
{
    public class Purchase
    {
        public int? Id { get; set; }
        public int StoreId { get; set; }
        public int CollectionId { get; set; }
        public PurchaseItem[] Items { get; set; }
        public PurchaseStatus Status { get; set; }
    }
}
