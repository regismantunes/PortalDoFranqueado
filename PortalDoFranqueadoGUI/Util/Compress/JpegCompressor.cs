using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using System.IO;
using System.Threading.Tasks;
using Aspose.Imaging.FileFormats.Jpeg;

namespace PortalDoFranqueadoGUI.Util.Compress
{
    public class JpegCompressor : ICompressor, ICompressorAsync
    {
        public CompressStrategy GetCompressStrategy()
            => CompressStrategy.Jpeg;

        public byte[] Compress(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var image = Image.Load(stream);
            var options = new JpegOptions()
            {
                ColorType = JpegCompressionColorMode.Grayscale,
                CompressionType = JpegCompressionMode.Progressive
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
