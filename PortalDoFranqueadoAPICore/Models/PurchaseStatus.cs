using System.ComponentModel;

namespace PortalDoFranqueadoAPICore.Models
{
    public enum PurchaseStatus
    {
        [Description("Nenhum")]
        None = -1,
        [Description("Aberta")]
        Opened = 0,
        [Description("Fechada")]
        Closed = 1,
        [Description("Encerrada")]
        Finished = 8
    }
}
