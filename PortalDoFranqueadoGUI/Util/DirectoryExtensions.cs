using System.IO;

namespace PortalDoFranqueadoGUI.Util
{
    public static class DirectoryExtensions
    {
        public static void CreateDirectoryChain(this DirectoryInfo directoryInfo)
        {
            if (directoryInfo.Exists)
                return;

            directoryInfo.Parent.CreateDirectoryChain();
            directoryInfo.Create();
        }

        public static void CreateDirectoryChain(string directoryPath)
            => CreateDirectoryChain(new DirectoryInfo(directoryPath));
    }
}
