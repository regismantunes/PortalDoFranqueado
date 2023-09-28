using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Linq;

namespace PortalDoFranqueado.ViewModel
{
    public abstract class FileViewViewModel : BaseViewModel
    {
        internal async Task LoadImageData(IEnumerable<FileView> files)
        {
            var hasError = false;
            var count = 0;
            var totalFiles = files.Count();
            foreach (var fileView in files)
            {
                try
                {
                    fileView.PrepareDirectory();
                    if (!fileView.FileExists)
                        await fileView.Download();

                    if (fileView.FileExists)
                        Me?.Dispatcher.BeginInvoke(() =>
                        {
                            count++;
                            Legendable?.SendMessage($"Carrengando fotos {count} de {totalFiles}...");
                            fileView.LoadImageData();
                        });
                }
                catch (Exception ex)
                {
                    if (!hasError)
                    {
                        hasError = true;
                        Me?.Dispatcher.BeginInvoke(() => MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar produtos", MessageBoxButton.OK, MessageBoxImage.Error));
                    }
                }
            }

            Me?.Dispatcher.BeginInvoke(() =>
            {
                Legendable?.SendMessage(string.Empty);
            });
        }
    }
}
