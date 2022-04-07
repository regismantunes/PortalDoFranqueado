using System;

namespace PortalDoFranqueado.Model
{
    public class Collection
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CollectionStatus Status { get; set; }
        public Product[]? Products { get; set; }
    }
}
