using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.View
{
    /// <summary>
    /// Interaction logic for ViewImage.xaml
    /// </summary>
    public partial class ViewImage : UserControl
    {
        public ViewImage(FileView file)
        {
            InitializeComponent();

            this.DataContext = new ViewImageViewModel(file);
        }
    }
}
