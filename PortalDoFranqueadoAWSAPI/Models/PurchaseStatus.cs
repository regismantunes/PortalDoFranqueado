using System.ComponentModel;

namespace PortalDoFranqueadoAPIAWS.Models
{
    public enum PurchaseStatus
    {
        [Description("Aberta")]
        Opened = 0,
        [Description("Fechada")]
        Closed = 1,
        [Description("Encerrada")]
        Finished = 8
    }
}
