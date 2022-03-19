﻿using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.Update;
using PortalDoFranqueadoGUI.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
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

        public Visibility VisibilityReturn => _controls.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityLogout { get => _visibilityLogout; private set { _visibilityLogout = value; OnPropertyChanged(); } }
        public RelayCommand ReturnCommand { get; }
        public RelayCommand<Window> LoadedCommand { get; }
        public RelayCommand ReloadCurrentViewCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public MainWindowViewModel()
        {
            _controls = new Stack<ContentControl>();

            API.Configuration.Current.SessionChanged += SessionChanged;

            ChangeCurrentView();

            ReturnCommand = new RelayCommand(() => ReturnNavigation());
            LoadedCommand = new RelayCommand<Window>(Loaded);
            ReloadCurrentViewCommand = new RelayCommand(ReloadCurrentView);
            LogoutCommand = new RelayCommand(Logout);

            _visibilityLogout = Visibility.Hidden;
            Title = "BROTHERS - Portal do Franqueado";
        }

        private void Logout()
        {
            API.Configuration.Current.DisconectSession();
        }

        private void Loaded(Window window)
        {
            Me = window;
            CurrentViewControlFocused = true;
            Task.Factory.StartNew(() => VerifyUpdate());
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
            VisibilityLogout = API.Configuration.Current.Session == null ? Visibility.Hidden : Visibility.Visible;
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
            CurrentViewControl = API.Configuration.Current.Session == null ? new Login() :
                                 API.Configuration.Current.Session.ResetPassword ? new ChangePassword() :
                                 API.Configuration.Current.Session.User.Role == UserRole.Manager ? new MainManager() : 
                                                                                                   new MainFranchisee();
        }

        public void NavigateTo(ContentControl control)
        {
            CurrentViewControl = control;
        }

        public ContentControl ReturnNavigation()
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

        public void SendMessage(string message)
        {
            StatusMessage = message;
        }
    }
}
