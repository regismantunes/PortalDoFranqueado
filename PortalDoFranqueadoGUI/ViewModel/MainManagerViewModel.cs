using PortalDoFranqueadoGUI.Model;
using System.Windows;
using System;
using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.View;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class MainManagerViewModel : BaseViewModel, IReloadable
    {
        private User? _user;

        private double _maxWidthInformativeText;
        private Visibility _visibilityInformativeText;
        private string _informativeTitle;
        private string _informativeText;

        private ContentControl? _currentViewControl;
        private bool _currentViewControlFocused;

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

        public RelayCommand LoadedCommand { get; }
        public RelayCommand PhotosCommand { get; }
        public RelayCommand SupportCommand { get; }
        public RelayCommand CampaignsCommand { get; }
        public RelayCommand UpdateInformativeCommand { get; }
        public RelayCommand PurchaseCommand { get; }
        public RelayCommand UsersCommand { get; }

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

            LoadedCommand = new RelayCommand(() =>
            {
                MaxWidthInformativeText = Me.ActualWidth - 70;
                Me.SizeChanged += Me_SizeChanged;

                UpdateSessionInformation();
                UpdateInformative();
            });
            PhotosCommand = new RelayCommand(OpenPhotos);
            SupportCommand = new RelayCommand(OpenSupport);
            CampaignsCommand = new RelayCommand(OpenCampaigns);
            UpdateInformativeCommand = new RelayCommand(UpdateInformative);
            PurchaseCommand = new RelayCommand(OpenPurchase);
            UsersCommand = new RelayCommand(OpenUsers);
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
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenUsers()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerUsers());
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

                Navigator.NavigateTo(new ManagerCollections());
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenPhotos()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Auxiliary, API.Configuration.Current.Session.AuxiliaryPhotoId.Value, "GERENCIAR FOTOS E VÍDEOS"));
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenSupport()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Auxiliary, API.Configuration.Current.Session.AuxiliarySupportId.Value, "GERENCIAR MATERIAL DE APOIO"));
            }
            finally
            {
                EnableContent();
            }
        }

        public ContentControl? CurrentViewControl
        {
            get => _currentViewControl;
            set
            {
                _currentViewControl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentViewControlVisibility));
                CurrentViewControlFocused = _currentViewControl != null;
            }
        }

        public Visibility CurrentViewControlVisibility => CurrentViewControl == null ? Visibility.Collapsed : Visibility.Visible;

        public bool CurrentViewControlFocused
        {
            get => _currentViewControlFocused;
            set
            {
                _currentViewControlFocused = value;
                OnPropertyChanged();
            }
        }

        public void OpenCampaigns()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerCampaigns());
            }
            finally
            {
                EnableContent();
            }
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