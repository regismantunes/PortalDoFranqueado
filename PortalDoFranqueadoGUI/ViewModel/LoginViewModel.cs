using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.Update;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    internal class LoginViewModel : BaseViewModel
    {
        private string? _errorMessage;
        private bool _emailLoginFocused;
        private bool _passwordFocused;
        private bool _loginIsEnabled;
        private string? _lockLoginMessage;
        private PersistentLocalRepository _persistCache;

        public string? EmailLogin { get; set; }
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        public bool EmailLoginFocused 
        { 
            get => _emailLoginFocused;
            set
            {
                _emailLoginFocused = value;
                OnPropertyChanged();
            }
        }
        public bool PasswordFocused
        {
            get => _passwordFocused;
            set
            {
                _passwordFocused = value;
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
        public string? LockLoginMessage
        {
            get => _lockLoginMessage;
            set
            {
                _lockLoginMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LockLoginMessageVisibility));
            }
        }
        public Visibility LockLoginMessageVisibility => string.IsNullOrEmpty(_lockLoginMessage) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HellcomeMessageVisibility { get; private set; }

        public RelayCommand<PasswordBox> LoginCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(async (PasswordBox passwordBox) => await Login(passwordBox));
            LoadedCommand = new RelayCommand(async () => await Loaded());
            LoginIsEnabled = false;
            HellcomeMessageVisibility = Visibility.Collapsed;
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

                var connectionValidateInto = await API.ApiMainScreen.ValidateConnection(currentVersion);

                EnableContent();

                if (!connectionValidateInto.IsCompatibleVersion)
                {
                    LockLoginMessage = "Essa versão do Portal Do Franqueado está desatualizada. Aguarde a nova versão ser baixada, feche a aplicação e abra novamente para efetuar o login.";
                }
                else if (!connectionValidateInto.IsServiceAvalible)
                {
                    LockLoginMessage = "O serviço está iniciando. Aguarde...";
                    await Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        await Loaded();
                    }).ConfigureAwait(false);
                }
                else
                {
                    LoginIsEnabled = true;
                    HellcomeMessageVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(HellcomeMessageVisibility));

                    _persistCache = (PersistentLocalRepository)App.Current.Resources["PersistCache"];
                    EmailLogin = _persistCache?.LastUserName;
                    OnPropertyChanged(nameof(EmailLogin));
                    if (string.IsNullOrEmpty(EmailLogin))
                        EmailLoginFocused = true;
                    else
                        PasswordFocused = true;
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
                    _persistCache.LastUserName = EmailLogin;
                    _persistCache.SaveAsync();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
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