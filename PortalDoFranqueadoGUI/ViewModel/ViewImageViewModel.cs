using PortalDoFranqueadoGUI.Model;
using System.Windows.Media;

namespace PortalDoFranqueadoGUI.ViewModel
{
    public class ViewImageViewModel : BaseViewModel
    {
        public ImageSource? Source { get; }

        public ViewImageViewModel(FileView file)
        {
            if(!file.IsImage)
                return;

            Source = file.ImageData;
        }
    }
}