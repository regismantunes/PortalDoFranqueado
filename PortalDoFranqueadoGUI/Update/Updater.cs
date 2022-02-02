using Squirrel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.Update
{
    internal static class Updater
    {
        private static readonly UpdateManager _manager = UpdateManager.GitHubUpdateManager(@"https://github.com/regismantunes/PortalDoFranqueado").Result;

        public static async Task<bool> HasUpdateAvailable()
        {
            var result = await _manager.CheckForUpdate();
            return result.ReleasesToApply.Any();
        }

        public static async Task Update() 
            => await _manager.UpdateApp();

        public static Version GetCurrentVersion()
            => _manager.CurrentlyInstalledVersion().Version;
    }
}
