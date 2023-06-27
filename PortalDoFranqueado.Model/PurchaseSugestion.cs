namespace PortalDoFranqueado.Model
{
    public class PurchaseSugestion
    {
        public int? Id { get; set; }
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public decimal? Target { get; set; }
        public decimal? AverageTicket { get; set;}
        public decimal? PartsPerService { get; set; }
        public decimal? Coverage { get; set; }
        public int? TotalSugestedItems { get; set; }
    }
}
