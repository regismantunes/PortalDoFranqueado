namespace PortalDoFranqueadoGUI.Model
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FolderId { get; set; }
        public CampaignStatus Status { get; set; }
    }
}
