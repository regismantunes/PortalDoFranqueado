using PortalDoFranqueadoGUI.Model;
using System.Windows;
using System.IO;
using System;
using PortalDoFranqueadoGUI.Util;
using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.View;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class MainManagerViewModel : BaseViewModel, IReloadable
    {
        private User? _user;

        private double _maxWidthInformativeText;
        private Visibility _visibilityInformativeText;
        private string _informativeTitle;
        private string _informativeText;

        public string WellcomeMessage { get; private set; }
        public string InformativeTitle
        {
            get => _informativeTitle;
            set
            {
                _informativeTitle = value;
                OnPropertyChanged();
                LoadInformative();
            }
        }
        public string InformativeText
        {
            get => _informativeText;
            set
            {
                _informativeText = value;
                OnPropertyChanged();
                LoadInformative();
            }
        }

        public RelayCommand PhotosCommand { get; }
        public RelayCommand SupportCommand { get; }
        public RelayCommand CampaignsCommand { get; }
        public RelayCommand UpdateInformativeCommand { get; }
        public RelayCommand PurchaseCommand { get; }

        public double MaxWidthInformativeText
        {
            get => _maxWidthInformativeText;
            private set
            {
                if (value > 0)
                {
                    _maxWidthInformativeText = value;
                    OnPropertyChanged();
                    VisibilityInformativeText = Visibility.Visible;
                }
                else
                {
                    VisibilityInformativeText = Visibility.Hidden;
                    _maxWidthInformativeText = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility VisibilityInformativeText
        {
            get => _visibilityInformativeText;
            private set
            {
                _visibilityInformativeText = value;
                OnPropertyChanged();
            }
        }

        public MainManagerViewModel()
        {
            MaxWidthInformativeText = 0;

            API.Configuration.Current.SessionConnected += SessionConnected;

            UpdateSessionInformation();
            UpdateInformative();

            PhotosCommand = new RelayCommand(OpenPhotos);
            SupportCommand = new RelayCommand(OpenSupport);
            CampaignsCommand = new RelayCommand(OpenCampaigns);
            UpdateInformativeCommand = new RelayCommand(UpdateInformative);
            PurchaseCommand = new RelayCommand(OpenPurchase);
        }

        public void SetWindow(Window main)
        {
            Me = main;
            MaxWidthInformativeText = Me.ActualWidth - 70;
            Me.SizeChanged += Me_SizeChanged;
        }

        private void Me_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidthInformativeText = e.NewSize.Width - 70;
        }

        private async void LoadInformative()
        {
            try
            {
                DesableContent();

                await API.ApiMainScreen.UpdateInformative(new Informative
                {
                    Title = InformativeTitle,
                    Text = InformativeText
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenPurchase()
        {
            try
            {
                DesableContent();

                Navigator.NextNavigate(new ManagerCollections());
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenPhotos()
        {
            if (MessageBox.Show("Deseja fazer o download das fotos e vídeos para uso nas redes sociais?", "BROTHERS - Fotos e Vídeos", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = API.Configuration.Current.Session.FilesRepository.GetFilesOnFotosFolder();
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Fotos e Vídeos");
                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    foreach (var file in files)
                        file.StartDownload(API.Configuration.Current.Session.FilesRepository.Drive, directoryToSave);

                    System.Diagnostics.Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        private void OpenSupport()
        {
            if (MessageBox.Show("Deseja fazer o download do material de apoio da marca?", "BROTHERS - Material de Apoio", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = API.Configuration.Current.Session.FilesRepository.GetFilesOnApoioFolder();
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Material de Apoio");
                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    foreach (var file in files)
                        file.StartDownload(API.Configuration.Current.Session.FilesRepository.Drive, directoryToSave);

                    System.Diagnostics.Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        public void OpenCampaigns()
        {

        }

        private void UpdateSessionInformation()
        {
            _user = API.Configuration.Current.Session?.User;

            WellcomeMessage = _user == null ? string.Empty : $"SEJA BEM VINDO {(!string.IsNullOrEmpty(_user.Treatment) ? _user.Treatment.ToUpper() + ' ' : string.Empty)} {_user.Name.ToUpper()}";

            OnPropertyChanged(nameof(WellcomeMessage));
        }

        public void Reload() => UpdateInformative();

        private async void UpdateInformative()
        {
            if (API.Configuration.Current.Session != null)
            {
                try
                {
                    DesableContent();

                    var mainInfos = await API.ApiMainScreen.GetBasicInfos();

                    _informativeTitle = mainInfos.InformativeTitle;
                    _informativeText = mainInfos.InformativeText;

                    OnPropertyChanged(nameof(InformativeTitle));
                    OnPropertyChanged(nameof(InformativeText));
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        private void SessionConnected(object? sender, EventArgs e)
        {
            UpdateSessionInformation();
            UpdateInformative();
        }
    }
}
