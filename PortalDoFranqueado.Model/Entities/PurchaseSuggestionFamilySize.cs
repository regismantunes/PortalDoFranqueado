namespace PortalDoFranqueado.Model.Entities
{
    public class PurchaseSuggestionFamilySize
    {
        public int? Id { get; set; }
        public int? PurchaseSuggestionFamilyId { get; set; }
        public PurchaseSuggestionFamily PurchaseSuggestionFamily { get; set; }
        public ProductSize Size { get; set; }
        public decimal Percentage { get; set; }
        public int? SizeSuggestedItems { get; set; }
    }
}
