using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class PurchaseSuggestion
    {
        public int? Id { get; set; }
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public decimal? Target { get; set; }
        public decimal? AverageTicket { get; set; }
        public int? PartsPerService { get; set; }
        public decimal? Coverage { get; set; }
        public int? TotalSuggestedItems { get; set; }
        public IEnumerable<PurchaseSuggestionFamily> Families { get; set; }
    }
}
