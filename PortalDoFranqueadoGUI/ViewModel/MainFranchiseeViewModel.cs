﻿using PortalDoFranqueado.Model;
using System.Windows;
using System;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Media.Imaging;
using PortalDoFranqueado.View;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;

namespace PortalDoFranqueado.ViewModel
{
    internal class MainFranchiseeViewModel : BaseViewModel, IReloadable
    {
        private User? _user;

        private double _maxWidthInformativeText;
        private Visibility _visibilityInformativeText;

        private StackPanel _stackPanelCampaigns;
        private readonly TemporaryLocalRepository _cache;

        public string WellcomeMessage { get; private set; }
        public string InformativeTitle { get; private set; }
        public string InformativeText { get; private set; }
        public bool EnabledPurchase { get; private set; }
        public string TextPurchase { get; private set; }

        public IEnumerable<Campaign> Campaigns { get; private set; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand PhotosCommand { get; }
        public RelayCommand SupportCommand { get; }
        public RelayCommand<Campaign> CampaignCommand { get; }
        public RelayCommand UpdateInfosCommand { get; }
        public RelayCommand PurchaseCommand { get; }
        public RelayCommand PurchaseSuggestionCommand { get; }

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
                LoadCampaigns(); 
                OnPropertyChanged();
            }
        }

        public MainFranchiseeViewModel()
        {
            _cache = (TemporaryLocalRepository)App.Current.Resources["TempCache"];

            MaxWidthInformativeText = 0;

            Api.Configuration.Current.SessionConnected += SessionConnected;

            LoadedCommand = new RelayCommand(() =>
            {
                UpdateSessionInformations();
                UpdateInformative();
            });
            PhotosCommand = new RelayCommand(OpenPhotos);
            SupportCommand = new RelayCommand(OpenSupport);
            CampaignCommand = new RelayCommand<Campaign>(OpenCampaign);
            UpdateInfosCommand = new RelayCommand(UpdateInformative);
            PurchaseCommand = new RelayCommand(OpenPurchase);
            PurchaseSuggestionCommand = new RelayCommand(OpenPurchaseSuggestion);
        }

        private void Me_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MaxWidthInformativeText = e.NewSize.Width - 70;
        }

        private void OpenPurchase()
        {
            try
            {
                if (_cache.Stores.Count == 0)
                {
                    MessageBox.Show(Me,"Nenhuma loja está vinculada ao seu perfil de usuário.");
                    return;
                }

                Navigator.NavigateTo(new FranchiseePurchaseStore());
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao abrir compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPurchaseSuggestion()
        {
            try
            {
                if (_cache.Stores.Count == 0)
                {
                    MessageBox.Show(Me, "Nenhuma loja está vinculada ao seu perfil de usuário.");
                    return;
                }

                Navigator.NavigateTo(new FranchiseePurchaseSuggestionStore());
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao abrir previsão de compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OpenPhotos()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Auxiliary, Api.Configuration.Current.Session.AuxiliaryPhotoId.Value, "FOTOS E VÍDEOS"));
            }
            finally
            {
                EnableContent();
            }

            /*if (MessageBox.Show(Me,"Deseja fazer o download das fotos e vídeos para uso nas redes sociais?", "BROTHERS - Fotos e Vídeos", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = await API.ApiFile.GetFromAuxiliary(API.Configuration.Current.Session.AuxiliaryPhotoId.Value);
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Fotos e Vídeos");
                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    files.AsParallel().ForAll(f => new FileView(f).StartDownload(directoryToSave));

                    System.Diagnostics.Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                finally
                {
                    EnableContent();
                }
            }*/
        }

        private async void OpenSupport()
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Auxiliary, Api.Configuration.Current.Session.AuxiliarySupportId.Value, "MATERIAL DE APOIO"));
            }
            finally
            {
                EnableContent();
            }

            /*if (MessageBox.Show(Me,"Deseja fazer o download do material de apoio da marca?", "BROTHERS - Material de Apoio", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = await API.ApiFile.GetFromAuxiliary(API.Configuration.Current.Session.AuxiliarySupportId.Value);
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Material de Apoio");
                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    files.AsParallel().ForAll(f => new FileView(f).StartDownload(directoryToSave));

                    System.Diagnostics.Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                finally
                {
                    EnableContent();
                }
            }*/
        }

        public async void OpenCampaign(Campaign campaign)
        {
            if (campaign == null)
                return;

            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Campaign, campaign.Id, $"CAMPANHA {campaign.Title}"));
            }
            finally
            {
                EnableContent();
            }

            /*if (MessageBox.Show(Me,$"Deseja fazer o download do material da campanha {campaign.Title}?", "BROTHERS - Campanhas", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DesableContent();

                    var files = await API.ApiFile.GetFromCampaign(campaign.Id);

                    var campaignDirName = string.Empty;
                    var invalidChars = Path.GetInvalidPathChars();
                    foreach (var c in campaign.Title)
                        campaignDirName += invalidChars.Contains(c) ? '_' : c;
                    var directoryToSave = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Campanhas", campaignDirName);

                    DirectoryExtensions.CreateDirectoryChain(directoryToSave);

                    files.AsParallel().ForAll(f => new FileView(f).StartDownload(directoryToSave));

                    Process.Start("explorer.exe", $"\"{directoryToSave}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao abrir material de campanha", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    EnableContent();
                }
            }*/
        }

        private void UpdateSessionInformations()
        {
            _user = Api.Configuration.Current.Session?.User;

            WellcomeMessage = _user == null ? string.Empty : $"SEJA BEM VINDO {(!string.IsNullOrEmpty(_user.Treatment) ? _user.Treatment.ToUpper() + ' ' : string.Empty)} {_user.Name.ToUpper()}";

            OnPropertyChanged(nameof(WellcomeMessage));
        }

        public void Reload() => UpdateInformative();

        private async void UpdateInformative()
        {
            if (Api.Configuration.Current.Session != null)
            {
                try
                {
                    DesableContent();

                    var mainInfos = await Api.ApiMainScreen.GetInfos();

                    InformativeTitle = mainInfos.InformativeTitle;
                    InformativeText = mainInfos.InformativeText;
                    EnabledPurchase = mainInfos.EnabledPurchase && 
                                      mainInfos.Stores.Any();
                    TextPurchase = mainInfos.TextPurchase;

                    Campaigns = mainInfos.Campaigns;

                    _cache.Stores = mainInfos.Stores.ToList();

                    LoadCampaigns();

                    OnPropertyChanged(nameof(InformativeTitle));
                    OnPropertyChanged(nameof(InformativeText));
                    OnPropertyChanged(nameof(EnabledPurchase));
                    OnPropertyChanged(nameof(TextPurchase));
                    OnPropertyChanged(nameof(Campaigns));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao atualizar informações", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    EnableContent();
                }
            }
        }

        private void LoadCampaigns()
        {
            if (StackPanelCampaigns == null ||
                Campaigns == null)
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

            foreach (Campaign campaign in Campaigns)
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

            MaxWidthInformativeText = Me.ActualWidth - 70;
            Me.SizeChanged += Me_SizeChanged;
        }

        private void SessionConnected(object? sender, EventArgs e)
        {
            UpdateSessionInformations();
            UpdateInformative();
        }
    }
}
