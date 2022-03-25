using System.ComponentModel;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public enum CompressStrategy
    {
        [Description("GZip")]
        GZip,
        [Description("None")]
        None,
        [Description("Png")]
        Png,
        [Description("Jpeg")]
        Jpeg,
    }
}
