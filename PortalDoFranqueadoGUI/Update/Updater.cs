using Squirrel;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Update
{
    internal static class Updater
    {
        private static readonly UpdateManager _manager = UpdateManager.GitHubUpdateManager(@"https://github.com/regismantunes/PortalDoFranqueado").Result;

        public static void Initialize()
        {
            SquirrelAwareApp.HandleEvents(
                  onInitialInstall: v =>
                  {
                      var assemblyName = Assembly.GetExecutingAssembly().FullName;
                      _manager.CreateShortcutsForExecutable(assemblyName, ShortcutLocation.Desktop, false);
                      _manager.CreateShortcutsForExecutable(assemblyName, ShortcutLocation.StartMenu, false);
                  },
                  onAppUpdate: v => _manager.CreateShortcutForThisExe(),
                  onAppUninstall: v => _manager.RemoveShortcutForThisExe());
        }

        public static async Task<bool> HasUpdateAvailable()
        {
            var result = await _manager.CheckForUpdate();
            return result.ReleasesToApply.Count != 0;
        }

        public static async Task Update() 
            => await _manager.UpdateApp();

        public static Version GetCurrentVersion()
            => _manager.CurrentlyInstalledVersion().Version;
    }
}
