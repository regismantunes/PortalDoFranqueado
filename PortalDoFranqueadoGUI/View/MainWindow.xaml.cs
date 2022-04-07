using PortalDoFranqueado.Repository;
using PortalDoFranqueado.ViewModel;
using System.Windows;

namespace PortalDoFranqueado.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Worker.GetActiveWorks() > 0)
            {
                e.Cancel = true;
                MessageBox.Show(this, "Existem arquivos que estão sendo salvos. Aguarde para fechar.", "BROTHERS - Portal do Franqueado", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
