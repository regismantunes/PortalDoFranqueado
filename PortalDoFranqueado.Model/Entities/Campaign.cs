using PortalDoFranqueado.Model.Enums;
using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public CampaignStatus Status { get; set; }
        public IEnumerable<MyFile>? Files { get; set; }
    }
}
