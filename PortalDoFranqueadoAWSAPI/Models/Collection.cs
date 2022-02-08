using System;

namespace PortalDoFranqueadoAPIAWS.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FolderId { get; set; }
        public CollectionStatus Status { get; set; }
    }
}
