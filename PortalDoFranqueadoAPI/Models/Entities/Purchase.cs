using PortalDoFranqueadoAPI.Models.Enums;
using System.Collections.Generic;

namespace PortalDoFranqueadoAPI.Models.Entities
{
    public class Purchase
    {
        public int? Id { get; set; }
        public int StoreId { get; set; }
        public int CollectionId { get; set; }
        public IEnumerable<PurchaseItem> Items { get; set; }
        public PurchaseStatus Status { get; set; }
    }
}
