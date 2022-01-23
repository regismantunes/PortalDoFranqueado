using PortalDoFranqueadoGUI.Model;
using System.Windows;
using System.IO;
using System;
using System.Linq;
using PortalDoFranqueadoGUI.Util;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Media.Imaging;
using PortalDoFranqueadoGUI.View;
using PortalDoFranqueadoGUI.Repository;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class MainFranchiseeViewModel : BaseViewModel, IReloadable
    {
        private User? _user;

        private double _maxWidthInformativeText;
        private Visibility _visibilityInformativeText;

        private StackPanel _stackPanelCampaigns;
        private readonly LocalRepository _cache;

        public string WellcomeMessage { get; private set; }
        public string InformativeTitle { get; private set; }
        public string InformativeText { get; private set; }
        public bool EnabledPurchase { get; private set; }
        public string TextPurchase { get; private set; }

        public MarketingCampaign[] Campaigns { get; private set; }

        public RelayCommand PhotosCommand { get; }
        public RelayCommand SupportCommand { get; }
        public RelayCommand<MarketingCampaign> CampaignCommand { get; }
        public RelayCommand UpdateInfosCommand { get; }
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

        public StackPanel StackPanelCampaigns
        {
            get => _stackPanelCampaigns; 
            set
            {
                _stackPanelCampaigns = value; 
                LoadMarketingCampaigns(); 
                OnPropertyChanged();
            }
        }

        public MainFranchiseeViewModel()
        {
            _cache = (LocalRepository)App.Current.Resources["Cache"];

            MaxWidthInformativeText = 0;

            API.Configuration.Current.PropertyChanged += Current_PropertyChanged;

            UpdateSessionInformations();
            UpdateInformative();

            PhotosCommand = new RelayCommand(OpenPhotos);
            SupportCommand = new RelayCommand(OpenSupport);
            CampaignCommand = new RelayCommand<MarketingCampaign>(OpenCampaign);
            UpdateInfosCommand = new RelayCommand(UpdateInformative);
            PurchaseCommand = new RelayCommand(OpenPurchases);
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

        private void OpenPurchases()
        {
            try
            {
                if (_cache.Stores.Count == 0)
                {
                    MessageBox.Show("Nenhuma loja está vinculada ao seu perfil de usuário.");
                    return;
                }

                Navigator.NextNavigate(new PurchaseStore());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BROTHERS - Falha ao abrir compras", MessageBoxButton.OK, MessageBoxImage.Error);
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BROTHERS - Falha ao abrir fotos", MessageBoxButton.OK, MessageBoxImage.Error);
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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BROTHERS - Falha ao abrir material de apoio", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        public void OpenCampaign(MarketingCampaign campaign)
        {
            if (campaign == null)
                return;

            if (MessageBox.Show($"Deseja fazer o download do material da campanha {campaign.Title}?", "BROTHERS - Campanhas", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = API.Configuration.Current.Session.FilesRepository.GetFilesOnFolder(campaign.FolderId);

                    var campaignDirName = string.Empty;
                    var invalidChars = Path.GetInvalidPathChars();
                    foreach (var c in campaign.Title)
                        campaignDirName += invalidChars.Contains(c) ? '_' : c;
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Campanhas", campaignDirName);

                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    foreach (var file in files)
                        file.StartDownload(API.Configuration.Current.Session.FilesRepository.Drive, directoryToSave);

                    System.Diagnostics.Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BROTHERS - Falha ao abrir material de campanha", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        private void UpdateSessionInformations()
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

                    var mainInfos = await API.ApiMainScreen.GetInfos();

                    InformativeTitle = mainInfos.InformativeTitle;
                    InformativeText = mainInfos.InformativeText;
                    EnabledPurchase = mainInfos.EnabledPurchase && 
                                      mainInfos.Stores.Length > 0;
                    TextPurchase = mainInfos.TextPurchase;

                    Campaigns = mainInfos.Campaigns;

                    _cache.Stores = mainInfos.Stores;

                    LoadMarketingCampaigns();

                    OnPropertyChanged(nameof(InformativeTitle));
                    OnPropertyChanged(nameof(InformativeText));
                    OnPropertyChanged(nameof(EnabledPurchase));
                    OnPropertyChanged(nameof(TextPurchase));
                    OnPropertyChanged(nameof(Campaigns));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BROTHERS - Falha ao atualizar informações", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        private void LoadMarketingCampaigns()
        {
            if (StackPanelCampaigns is null ||
                Campaigns is null)
                return;

            var oldButtons = (from UIElement b in StackPanelCampaigns.Children
                              where b.GetType() == typeof(Button)
                              select b).ToArray();

            foreach (var button in oldButtons)
                StackPanelCampaigns.Children.Remove(button);

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("/PortalDoFranqueadoGUI;component/Media/media.png", UriKind.Relative);
            image.EndInit();

            foreach (MarketingCampaign campaign in Campaigns)
            {
                var stackPanelContent = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                stackPanelContent.Children.Add(
                    new Image()
                    {
                        Source = image,
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                stackPanelContent.Children.Add(
                    new TextBlock()
                    {
                        Text = campaign.Title,
                        Style = Application.Current.Resources["TextBlockMinButton"] as Style,
                        MaxWidth = 205
                    });

                var button = new Button()
                {
                    Style = Application.Current.Resources["MainButton"] as Style,
                    Command = CampaignCommand,
                    CommandParameter = campaign,
                    Content = stackPanelContent
                };

                StackPanelCampaigns.Children.Add(button);
            }
        }

        private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Session")
            {
                UpdateSessionInformations();
                UpdateInformative();
            }
        }
    }
}
