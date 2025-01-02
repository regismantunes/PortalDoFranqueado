using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    internal class ChangePasswordViewModel : BaseViewModel
    {
        private bool _currentPasswordFocused;
        private bool _password1Focused;
        private bool _password2Focused;

        public string MensagemUsuario { get; }
        public Visibility VisibilityCurrentPassword { get; }
        public string? ErrorMessage { get; private set; }
        public bool CurrentPasswordFocused
        {
            get => _currentPasswordFocused;
            set
            {
                _currentPasswordFocused = value;
                OnPropertyChanged();
            }
        }
        public bool Password1Focused
        {
            get => _password1Focused;
            set
            {
                _password1Focused = value;
                OnPropertyChanged();
            }
        }
        public bool Password2Focused
        {
            get => _password2Focused;
            set
            {
                _password2Focused = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<StackPanel> ChangePasswordCommand { get; }
        public RelayCommand LoadedCommand { get; }

        public ChangePasswordViewModel()
        {
            var user = Api.Configuration.Current.Session?.User;
            var resetPassword = Api.Configuration.Current.Session?.ResetPassword ?? false;

            MensagemUsuario = $"{user?.Name}, informe sua nova senha.";
            VisibilityCurrentPassword = resetPassword ? Visibility.Collapsed : Visibility.Visible;

            ChangePasswordCommand = new RelayCommand<StackPanel>(ChangePassword);
            LoadedCommand = new RelayCommand(() =>
            {
                if (resetPassword)
                    Password1Focused = true;
                else
                    CurrentPasswordFocused = true;
            });
        }

        public async void ChangePassword(StackPanel? spPasswords)
        {
            if (spPasswords is null)
                return;

            try
            {
                DesableContent();

                var currentPassword = ((PasswordBox)spPasswords.Children[0]).Password;
                var password1 = ((PasswordBox)spPasswords.Children[1]).Password;
                var password2 = ((PasswordBox)spPasswords.Children[2]).Password;

                if (string.IsNullOrEmpty(currentPassword) &&
                    VisibilityCurrentPassword == Visibility.Visible)
                {
                    ErrorMessage = "Informe sua senha atual";
                    CurrentPasswordFocused = true;
                    return;
                }

                if (string.IsNullOrEmpty(password1))
                {
                    ErrorMessage = "Informe a nova senha";
                    Password1Focused = true;
                    return;
                }

                if (string.IsNullOrEmpty(password2))
                {
                    ErrorMessage = "Confirme sua nova senha";
                    Password2Focused = true;
                    return;
                }

                if (password1 != password2)
                {
                    ErrorMessage = "A confirmação da senha está divergente";
                    Password1Focused = true;
                    return;
                }

                try
                {
                    await Api.ApiAccount.ChangePassword(currentPassword, password1, password2);

                    if (Api.Configuration.Current.Session != null)
                        Api.Configuration.Current.Session.ResetPassword = false;
                    Api.Configuration.Current.NotifySessionChange();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
