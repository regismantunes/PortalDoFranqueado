using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using PortalDoFranqueado.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerCampaignsViewModel : BaseViewModel, IReloadable
    {
        public class CampaignViewModel : BaseNotifyPropertyChanged
        {
            private Campaign _campaign;
            private string _nextStatusAction;
            private bool _enabledNextStatus;
            private RelayCommand<CampaignViewModel>? _nextStatusCommand;
            private string _previusStatusAction;
            private bool _enabledPreviusStatus;
            private RelayCommand<CampaignViewModel>? _previusStatusCommand;
            private RelayCommand<CampaignViewModel> _deleteCommand;

            public CampaignViewModel(Campaign campaign)
                => _campaign = campaign ?? throw new ArgumentNullException(nameof(campaign));

            public Campaign Campaign { get => _campaign; set { _campaign = value; OnPropertyChanged(); } }
            public RelayCommand<Campaign> FilesCommand { get; set; }
            public string NextStatusAction { get => _nextStatusAction; set { _nextStatusAction = value; OnPropertyChanged(); } }
            public bool EnabledNextStatus { get => _enabledNextStatus; set { _enabledNextStatus = value; OnPropertyChanged(); } }
            public RelayCommand<CampaignViewModel>? NextStatusCommand { get => _nextStatusCommand; set { _nextStatusCommand = value; OnPropertyChanged(); } }
            public string PreviusStatusAction { get => _previusStatusAction; set { _previusStatusAction = value; OnPropertyChanged(); } }
            public bool EnabledPreviusStatus { get => _enabledPreviusStatus; set { _enabledPreviusStatus = value; OnPropertyChanged(); } }
            public RelayCommand<CampaignViewModel>? PreviusStatusCommand { get => _previusStatusCommand; set { _previusStatusCommand = value; OnPropertyChanged(); } }
            public RelayCommand<CampaignViewModel> DeleteCommand { get => _deleteCommand; set { _deleteCommand = value; OnPropertyChanged(); } }
        }

        public class NewCampaign
        {
            public FieldViewModel<string?> Title { get; } = new FieldViewModel<string?>();
        }

        public ObservableCollection<CampaignViewModel> Campaigns { get; }
        public NewCampaign CampaignToAdd { get; }

        public RelayCommand LoadedCommand { get; }
        public RelayCommand AddCampaignCommand { get; }
        
        public ManagerCampaignsViewModel()
        {
            Campaigns = new ObservableCollection<CampaignViewModel>();
            CampaignToAdd = new NewCampaign();

            LoadedCommand = new RelayCommand(LoadCampaigns);
            AddCampaignCommand = new RelayCommand(AddCampaign);
        }

        private async void LoadCampaigns()
        {
            try
            {
                DesableContent();

                Campaigns.Clear();

                var campaigns = await Api.ApiCampaign.GetCampaigns();

                foreach (var campaign in campaigns)
                    AddCampaign(campaign);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar campanhas", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async void AddCampaign()
        {
            try
            {
                const string messageCaption = "BROTHERS - Falta informação";
                DesableContent();

                if (string.IsNullOrEmpty(CampaignToAdd.Title.Value))
                {
                    MessageBox.Show(Me,"Informe o título da campanha!", messageCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                    CampaignToAdd.Title.IsFocused = true;
                }
                else
                {
                    var campaign = new Campaign
                    {
                        Title = CampaignToAdd.Title.Value,
                        Status = CampaignStatus.Holding
                    };

                    campaign.Id = await Api.ApiCampaign.Insert(campaign);

                    AddCampaign(campaign);

                    CampaignToAdd.Title.Value = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao adicionar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void AddCampaign(Campaign campaign)
            => Campaigns.Add(UpdateCampaign(new CampaignViewModel(campaign)));

        private CampaignViewModel UpdateCampaign(CampaignViewModel campaignVM)
        {
            var campaign = campaignVM.Campaign;
            campaignVM.FilesCommand = new RelayCommand<Campaign>(CampaignsFiles);
            campaignVM.NextStatusAction = campaign.Status switch
            {
                CampaignStatus.Holding => "Abrir",
                CampaignStatus.Opened => "Encerrar",
                _ => string.Empty
            };
            campaignVM.EnabledNextStatus = campaign.Status != CampaignStatus.Finished;
            campaignVM.NextStatusCommand = campaign.Status switch
            {
                CampaignStatus.Holding => new RelayCommand<CampaignViewModel>(OpenCampaign),
                CampaignStatus.Opened => new RelayCommand<CampaignViewModel>(FinishCampaign),
                _ => null
            };
            campaignVM.PreviusStatusAction = campaign.Status switch
            {
                CampaignStatus.Opened => "Estornar",
                CampaignStatus.Finished => "Reabrir",
                _ => string.Empty
            };
            campaignVM.EnabledPreviusStatus = campaign.Status != CampaignStatus.Holding;
            campaignVM.PreviusStatusCommand = campaign.Status switch
            {
                CampaignStatus.Opened => new RelayCommand<CampaignViewModel>(ReverseCampaign),
                CampaignStatus.Finished => new RelayCommand<CampaignViewModel>(OpenCampaign),
                _ => null
            };
            campaignVM.DeleteCommand = new RelayCommand<CampaignViewModel>(DeleteCampaign);
            campaignVM.Campaign = campaign; //Set campaign to force update on ViewModel
            return campaignVM;
        }

        public void CampaignsFiles(Campaign campaign)
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerFiles(FileOwner.Campaign, campaign.Id, $"GERENCIAR CAMPANHA {campaign.Title}"));
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenCampaign(CampaignViewModel campaignVM)
            => ChangeStatus(campaignVM, CampaignStatus.Opened);

        private async void ChangeStatus(CampaignViewModel campaignVM, CampaignStatus status)
        {
            try
            {
                DesableContent();

                await Api.ApiCampaign.ChangeStatus(campaignVM.Campaign.Id, status);

                campaignVM.Campaign.Status = status;
                UpdateCampaign(campaignVM);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao alterar situação", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void FinishCampaign(CampaignViewModel campaignVM)
            => ChangeStatus(campaignVM, CampaignStatus.Finished);

        private void ReverseCampaign(CampaignViewModel campaignVM)
            => ChangeStatus(campaignVM, CampaignStatus.Holding);

        private async void DeleteCampaign(CampaignViewModel campaignVM)
        {
            try
            {
                DesableContent();

                if (MessageBox.Show(Me,"Deseja realmente excluir?", "BROTHERS - Excluir", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    await Api.ApiCampaign.Delete(campaignVM.Campaign.Id);
                    Campaigns.Remove(campaignVM);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        public void Reload() => LoadCampaigns();
    }
}
