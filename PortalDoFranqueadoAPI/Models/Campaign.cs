namespace PortalDoFranqueadoAPI.Models
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public CampaignStatus Status { get; set; }
        public MyFile[]? Files { get; set; }
    }
}
