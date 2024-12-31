using System.ComponentModel;

namespace PortalDoFranqueadoAPI.Enums
{
    public enum CollectionStatus
    {
        [Description("Pendente")]
        Pendding = 0,
        [Description("Aberta")]
        Opened = 1,
        [Description("Encerrada")]
        Closed = 8
    }
}
