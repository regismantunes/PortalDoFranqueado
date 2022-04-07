using System.ComponentModel;

namespace PortalDoFranqueado.Model
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
