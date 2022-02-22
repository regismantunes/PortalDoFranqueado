using System.ComponentModel;

namespace PortalDoFranqueadoGUI.Model
{
    public enum CampaignStatus
    {
        [Description("Aguardando")]
        Holding = 0,
        [Description("Aberta")]
        Opened = 1,
        [Description("Encerrada")]
        Finished = 8
    }
}
