namespace PortalDoFranqueadoGUI.Util.Compress
{
    public interface ICompressor
    {
        CompressStrategy GetCompressStrategy();
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
}
