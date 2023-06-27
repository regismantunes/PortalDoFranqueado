namespace PortalDoFranqueado.Model
{
    public class PurchaseSugestionFamily
    {
        public int? Id { get; set; }
        public int? PurchaseSugestionId { get; set; }
        public PurchaseSugestion PurchaseSugestion { get; set; }
        public int FamilyId { get; set; }
        public Family Family { get; set; }
        public decimal Percentage { get; set; }
        public int? FamilySugestedItems { get; set; }
    }
}
