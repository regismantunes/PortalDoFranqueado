using CommunityToolkit.Mvvm.Input;
using PortalDoFranqueado.Repository;
using PortalDoFranqueado.Update;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
        private DispatcherTimer? _dTimer;

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
        public Visibility LockLoginMessageVisibility => string.IsNullOrEmpty(_lockLoginMessage) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility LoginVisibility { get; private set; }

        public RelayCommand<PasswordBox> LoginCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(async (PasswordBox passwordBox) => await Login(passwordBox));
            LoadedCommand = new RelayCommand(async () => await Loaded());
            LoginIsEnabled = false;
            LoginVisibility = Visibility.Collapsed;
            _lockLoginMessage = "Verificando conexões...";
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

                await ValidateConnection(currentVersion);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private async Task ValidateConnection(string currentVersion)
        {
            var connectionValidateInto = await Api.ApiMainScreen.ValidateConnection(currentVersion);

            EnableContent();

            if (!connectionValidateInto.IsCompatibleVersion)
            {
                LockLoginMessage = "Essa versão do Portal Do Franqueado está desatualizada. Aguarde a nova versão ser baixada, feche a aplicação e abra novamente para efetuar o login.";
                _dTimer?.Stop();
                _dTimer = null;
            }
            else if (!connectionValidateInto.IsServiceAvalible)
            {
                LockLoginMessage = "O serviço está iniciando. Aguarde...";
                if (_dTimer == null)
                {
                    _dTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 10) };
                    _dTimer.Tick += async (object? sender, EventArgs e) => await ValidateConnection(currentVersion);
                    _dTimer.Start();
                }
            }
            else
            {
                _dTimer?.Stop();
                _dTimer = null;

                LockLoginMessage = null;
                LoginIsEnabled = true;
                LoginVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(LoginVisibility));

                _persistCache = (PersistentLocalRepository)App.Current.Resources["PersistCache"];
                EmailLogin = _persistCache?.LastUserName;
                OnPropertyChanged(nameof(EmailLogin));
                if (string.IsNullOrEmpty(EmailLogin))
                    EmailLoginFocused = true;
                else
                    PasswordFocused = true;
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
                    await Api.ApiAccount.Login(EmailLogin, password);
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