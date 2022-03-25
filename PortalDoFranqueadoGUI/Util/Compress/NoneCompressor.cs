using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public class NoneCompressor : ICompressor, ICompressorAsync
    {
        public CompressStrategy GetCompressStrategy()
            => CompressStrategy.None;

        public byte[] Compress(byte[] data)
            => data;

        public byte[] Decompress(byte[] data)
            => data;

        public async Task<byte[]> CompressAsync(byte[] data)
            => data;

        public async Task<byte[]> DecompressAsync(byte[] data)
            => data;
    }
}
