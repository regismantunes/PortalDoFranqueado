using PortalDoFranqueado.Model;
using PortalDoFranqueado.ViewModel;
using System.Windows.Controls;

namespace PortalDoFranqueado.View
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
