using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class LoginViewModel : BaseViewModel
    {
        private bool _emailLoginFocused;

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

        public RelayCommand<PasswordBox> LoginCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(Login);
            LoadedCommand = new RelayCommand(() => EmailLoginFocused = true);
        }

        public async void Login(PasswordBox passwordBox)
        {
            try
            {
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
            }
        }
    }
}
