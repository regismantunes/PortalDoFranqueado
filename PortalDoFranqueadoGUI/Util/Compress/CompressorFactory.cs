namespace PortalDoFranqueadoGUI.Util.Compress
{
    public static class CompressorFactory
    {
        public static ICompressorAsync GetCompressorForMimeTypeAsync(string mimeType)
        {
            return new NoneCompressor();
        }

        public static ICompressorAsync GetCompressorAsync(string strategy)
        {
            if (strategy == "GZip")
                return new GZipCompressor();
            
            return new NoneCompressor();
        }
    }
}
