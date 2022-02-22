namespace PortalDoFranqueadoGUI.Model
{
    public class MainInfos
    {
        public string InformativeTitle { get; set; }
        public string InformativeText { get; set; }
        public bool EnabledPurchase { get; set; }
        public string TextPurchase { get; set; }

        public string GoogleDriveClientSecret { get; set; }
        public string GoogleDriveServiceCredentials { get; set; }
        public string GoogleDriveApplicationName { get; set; }
        public string PhotosFolderId { get; set; }
        public string SupportFolderId { get; set; }

        public Campaign[] Campaigns { get; set; }
        public Store[] Stores { get; set; }
    }
}
