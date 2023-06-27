namespace PortalDoFranqueado.Model
{
    public class PurchaseSugestionFamilySize
    {
        public int? Id { get; set; }
        public int? PurchaseSugestionFamilyId { get; set; }
        public PurchaseSugestionFamily PurchaseSugestionFamily { get; set; }
        public ProductSize Size { get; set; }
        public decimal Percentage { get; set; }
        public int? SizeSugestedItems { get; set; }
    }
}
