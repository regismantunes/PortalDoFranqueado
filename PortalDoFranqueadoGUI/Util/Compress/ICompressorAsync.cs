using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public interface ICompressorAsync
    {
        CompressStrategy GetCompressStrategy();
        Task<byte[]> CompressAsync(byte[] data);
        Task<byte[]> DecompressAsync(byte[] data);
    }
}
