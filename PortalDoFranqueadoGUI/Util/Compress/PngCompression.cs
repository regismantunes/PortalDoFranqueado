using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using System.IO;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public class PngCompression : ICompressor, ICompressorAsync
    {
        public CompressStrategy GetCompressStrategy()
            => CompressStrategy.Png;

        public byte[] Compress(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var image = Image.Load(stream);
            var options = new PngOptions
            {
                CompressionLevel = 5
            };
            using var outStream = new MemoryStream();
            image.Save(outStream, options);
            outStream.Position = 0;
            return outStream.ToArray();
        }

        public byte[] Decompress(byte[] data)
            => data;

        public async Task<byte[]> CompressAsync(byte[] data)
            => await Task<byte[]>.Factory.StartNew(() => Compress(data));

        public async Task<byte[]> DecompressAsync(byte[] data)
            => data;
    }
}
