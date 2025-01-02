using CommunityToolkit.Mvvm.Input;
using PortalDoFranqueado.Model.Enums;
using PortalDoFranqueado.Update;
using PortalDoFranqueado.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueado.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel, INavigatorViewModel, ILegendable
    {
        private readonly Stack<ContentControl> _controls;
        private bool _currentViewControlFocused;
        private string _statusMessage;

        public ContentControl CurrentViewControl 
        { 
            get => _controls.Peek();
            set
            {
                if (value.DataContext is INavigableViewModel navigable)
                    navigable.Navigator = this;

                if (value.DataContext is BaseViewModel baseViewModel)
                    baseViewModel.Me = Me;

                if (value is ChangePassword)
                    VisibilityChagePassword = Visibility.Collapsed;

                _controls.Push(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(VisibilityReturn));
                CurrentViewControlFocused = true;
            }
        }

        public bool CurrentViewControlFocused
        {
            get => _currentViewControlFocused;
            set
            {
                _currentViewControlFocused = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string CurrentVersion { get; private set; }
        public string Title { get; private set; }

        public Visibility _visibilityLogout;
        public Visibility _visibilityChagePassword;

        public Visibility VisibilityReturn => _controls.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityLogout { get => _visibilityLogout; private set { _visibilityLogout = value; OnPropertyChanged(); } }
        public Visibility VisibilityChagePassword { get => _visibilityChagePassword; private set { _visibilityChagePassword = value; OnPropertyChanged(); } }
        public RelayCommand ReturnCommand { get; }
        public RelayCommand<Window> LoadedCommand { get; }
        public RelayCommand ReloadCurrentViewCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand ChangePasswordCommand { get; }

        public MainWindowViewModel()
        {
            _controls = new Stack<ContentControl>();

            Api.Configuration.Current.SessionChanged += SessionChanged;

            ChangeCurrentView();

            ReturnCommand = new RelayCommand(() => ReturnNavigation());
            LoadedCommand = new RelayCommand<Window>(Loaded);
            ReloadCurrentViewCommand = new RelayCommand(ReloadCurrentView);
            LogoutCommand = new RelayCommand(Logout);
            ChangePasswordCommand = new RelayCommand(ChangePassword);

            _visibilityLogout = Visibility.Hidden;
            _visibilityChagePassword = Visibility.Hidden;
            Title = "BROTHERS - Portal do Franqueado";
        }

        private void Logout()
        {
            Api.Configuration.Current.DisconectSession();
        }

        private void ChangePassword()
        { 
            try
            {
                DesableContent();

                NavigateTo(new ChangePassword());
            }
            finally
            {
                EnableContent();
            }
        }

        private void Loaded(Window window)
        {
            Me = window;
            CurrentViewControlFocused = true;
            Task.Factory.StartNew(VerifyUpdate);
        }

        public async Task VerifyUpdate()
        {
            try
            {
                try
                {
                    CurrentVersion = Updater.GetCurrentVersion().ToString(3);
                }
                catch
                {
                    CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
                }
                Title = $"BROTHERS - Portal do Franqueado {CurrentVersion}";
                OnPropertyChanged(nameof(CurrentVersion));
                OnPropertyChanged(nameof(Title));

                StatusMessage = "Verificando atualizações...";
                if (await Updater.HasUpdateAvailable())
                {
                    StatusMessage = "Baixando uma atualização...";
                    await Updater.Update();
                    StatusMessage = "A atualização será instalada quando você fechar o aplicativo.";
                }
                else
                    StatusMessage = "Nenhuma atualização foi encontrada.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Erro ao verificar versão: " + ex.Message;
            }
        }

        private void SessionChanged(object? sender, EventArgs e)
        {
            VisibilityLogout = Api.Configuration.Current.Session == null ? Visibility.Hidden : Visibility.Visible;
            VisibilityChagePassword = VisibilityLogout;
            ChangeCurrentView();
        }

        private void ReloadCurrentView()
        {
            if (CurrentViewControl.DataContext is IReloadable reloadable)
                reloadable.Reload();
        }

        private void ChangeCurrentView()
        {
            _controls.Clear();
            CurrentViewControl = Api.Configuration.Current.Session == null ? new Login() :
                                 Api.Configuration.Current.Session.ResetPassword ? new ChangePassword() :
                                 Api.Configuration.Current.Session.User.Role == UserRole.Manager ? new MainManager() : 
                                                                                                   new MainFranchisee();
        }

        public void NavigateTo(ContentControl control)
        {
            CurrentViewControl = control;
        }

        public bool ReturnNavigation()
        {
            var previus = _controls.Peek();

            if (previus.DataContext is INavigableViewModel navigable)
                if (!navigable.BeforeReturn())
                    return false;

            if (previus is ChangePassword)
                VisibilityChagePassword = Visibility.Visible;

            if (previus.DataContext is IDisposable disposable)
                disposable.Dispose();

            _controls.Pop();

            OnPropertyChanged(nameof(CurrentViewControl));
            OnPropertyChanged(nameof(VisibilityReturn));

            var current = CurrentViewControl;
            if (current.DataContext is INavigableViewModel currentNavigable)
                currentNavigable.OnReturnToView();

            return true;
        }

        public void SendMessage(string message)
        {
            StatusMessage = message;
            //App.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
