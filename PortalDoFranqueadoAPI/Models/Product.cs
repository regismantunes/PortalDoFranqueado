namespace PortalDoFranqueadoAPI.Models
{
    public class Product
    {
        public int? Id { get; set; }
        public decimal? Price { get; set; }
        public int? FamilyId { get; set; }
        public string[]? LockedSizes { get; set; }
        public int FileId { get; set; }
        public int? SupplierId { get; set; }
    }
}
