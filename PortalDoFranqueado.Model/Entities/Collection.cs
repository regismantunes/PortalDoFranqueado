using PortalDoFranqueado.Model.Enums;
using System;
using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class Collection
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CollectionStatus Status { get; set; }
        public IEnumerable<Product>? Products { get; set; }
    }
}
