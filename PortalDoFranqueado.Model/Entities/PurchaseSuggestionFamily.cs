using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class PurchaseSuggestionFamily
    {
        public int? Id { get; set; }
        public int? PurchaseSuggestionId { get; set; }
        public PurchaseSuggestion PurchaseSuggestion { get; set; }
        public int FamilyId { get; set; }
        public Family Family { get; set; }
        public decimal Percentage { get; set; }
        public int? FamilySuggestedItems { get; set; }
        public IEnumerable<PurchaseSuggestionFamilySize> Sizes { get; set; }
    }
}
