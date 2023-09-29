using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Linq;

namespace PortalDoFranqueado.ViewModel
{
    public abstract class FileViewViewModel : BaseViewModel
    {
        internal async Task LoadFiles(IEnumerable<FileView> files)
        {
            var hasError = false;
            var count = 0;
            var totalFiles = files.Count();
            foreach (var fileView in files)
            {
                try
                {
                    await fileView.Download();

                    fileView.GenerateMiniature();

                    Me?.Dispatcher.BeginInvoke(() =>
                    {
                        count++;
                        Legendable?.SendMessage($"Carrengando fotos {count} de {totalFiles}...");
                    });

                    if (!fileView.FileExists)
                        throw new Exception($"Falha ao carregar arquivo {fileView.FilePath}");
                }
                catch (Exception ex)
                {
                    if (!hasError)
                    {
                        hasError = true;
                        Me?.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar produtos", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
            }

            Me?.Dispatcher.BeginInvoke(() => Legendable?.SendMessage(string.Empty));
        }
    }
}
