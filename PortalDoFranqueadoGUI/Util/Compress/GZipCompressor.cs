using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public class GZipCompressor : ICompressor, ICompressorAsync
    {
        public CompressStrategy GetCompressStrategy()
            => CompressStrategy.GZip;

        public byte[] Compress(byte[] data)
        {
            var task = CompressAsync(data);
            task.Wait();
            return task.Result;
        }

        public byte[] Decompress(byte[] data)
        {
            var task = DecompressAsync(data);
            task.Wait();
            return task.Result;
        }

        public async Task<byte[]> CompressAsync(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            await zipStream.WriteAsync(data);
            zipStream.Close();
            return compressedStream.ToArray();
        }

        public async Task<byte[]> DecompressAsync(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            await zipStream.CopyToAsync(resultStream);
            return resultStream.ToArray();
        }
    }
}
