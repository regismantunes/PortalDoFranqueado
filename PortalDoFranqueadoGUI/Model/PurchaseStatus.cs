using System.ComponentModel;

namespace PortalDoFranqueadoGUI.Model
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
