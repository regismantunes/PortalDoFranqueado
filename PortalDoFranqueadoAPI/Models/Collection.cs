using PortalDoFranqueadoAPI.Models.Enums;
using System;

namespace PortalDoFranqueadoAPI.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CollectionStatus Status { get; set; }
    }
}
