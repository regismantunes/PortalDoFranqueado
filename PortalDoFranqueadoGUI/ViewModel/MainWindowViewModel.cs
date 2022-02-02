using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Update;
using PortalDoFranqueadoGUI.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel, INavigatorViewModel
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

        public Visibility VisibilityReturn => _controls.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ReturnCommand { get; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand ReloadCurrentViewCommand { get; }

        public MainWindowViewModel()
        {
            _controls = new Stack<ContentControl>();

            API.Configuration.Current.SessionChanged += SessionChanged;

            ChangeCurrentView();

            ReturnCommand = new RelayCommand(() => PreviousNavigate());
            LoadedCommand = new RelayCommand(Loaded);
            ReloadCurrentViewCommand = new RelayCommand(ReloadCurrentView);

            Title = "BROTHERS - Portal do Franqueado";
        }

        private void Loaded()
        {
            CurrentViewControlFocused = true;
            Task.Run(async () => await VerifyUpdate());
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
            CurrentViewControl = API.Configuration.Current.Session == null ?    new Login() :
                API.Configuration.Current.Session.User.Role == "manager" ?      new MainManager() : 
                                                                                new MainFranchisee();
        }

        public void NextNavigate(ContentControl control)
        {
            CurrentViewControl = control;
        }

        public ContentControl PreviousNavigate()
        {
            var previus = _controls.Pop();

            if (previus.DataContext is IDisposable disposable)
                disposable.Dispose();

            OnPropertyChanged(nameof(CurrentViewControl));
            OnPropertyChanged(nameof(VisibilityReturn));

            var current = CurrentViewControl;
            if (current.DataContext is INavigableViewModel navigable)
                navigable.OnReturnToView();

            return current;
        }
    }
}
