using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Update;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    internal class LoginViewModel : BaseViewModel
    {
        private bool _emailLoginFocused;
        private bool _loginIsEnabled;

        public string? EmailLogin { get; set; }
        public string? ErrorMessage { get; private set; }
        public bool EmailLoginFocused 
        { 
            get => _emailLoginFocused;
            set
            {
                _emailLoginFocused = value;
                OnPropertyChanged();
            }
        }
        public bool LoginIsEnabled
        {
            get => _loginIsEnabled;
            set
            {
                _loginIsEnabled = value;
                OnPropertyChanged();
            }
        }
        public Visibility OldVersionMessageVisibility { get; private set; }
        public Visibility HellcomeMessageVisibility { get; private set; }

        public RelayCommand<PasswordBox> LoginCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(async (PasswordBox passwordBox) => await Login(passwordBox));
            LoadedCommand = new RelayCommand(async () => await Loaded());
            LoginIsEnabled = false;
            HellcomeMessageVisibility = Visibility.Collapsed;
            OldVersionMessageVisibility = Visibility.Collapsed;
        }

        private async Task Loaded()
        {
            try
            {
                DesableContent();

                var currentVersion = string.Empty;
                try
                {
                    currentVersion = Updater.GetCurrentVersion().ToString(3);
                }
                catch
                {
                    currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                }

                var isCompatible = await API.ApiMainScreen.VerifyCompatibleVersion(currentVersion);

                EnableContent();

                if (isCompatible)
                {
                    LoginIsEnabled = true;
                    EmailLoginFocused = true;
                    HellcomeMessageVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(HellcomeMessageVisibility));
                }
                else
                {
                    OldVersionMessageVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(OldVersionMessageVisibility));
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private async Task Login(PasswordBox passwordBox)
        {
            try
            {
                LoginIsEnabled = false;
                DesableContent();

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    ErrorMessage = "Informe o email de login";
                    EmailLoginFocused = true;
                    return;
                }

                var password = passwordBox.Password;

                if (string.IsNullOrEmpty(password))
                {
                    ErrorMessage = "Informe a senha";
                    return;
                }

                try
                {
                    await API.ApiAccount.Login(EmailLogin, password);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    OnPropertyChanged(nameof(ErrorMessage));
                    EmailLoginFocused = true;
                }
            }
            finally
            {
                EnableContent();
                LoginIsEnabled = true;
            }
        }
    }
}