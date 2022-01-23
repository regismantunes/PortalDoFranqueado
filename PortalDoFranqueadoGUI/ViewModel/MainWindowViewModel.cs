using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.View;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel, INavigatorViewModel
    {
        private readonly Stack<ContentControl> _controls;
        private bool _currentViewControlFocused;

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

        public Visibility VisibilityReturn => _controls.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ReturnCommand { get; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand ReloadCurrentViewCommand { get; }

        public MainWindowViewModel()
        {
            _controls = new Stack<ContentControl>();

            API.Configuration.Current.PropertyChanged += Current_PropertyChanged;

            ChangeCurrentView();

            ReturnCommand = new RelayCommand(() => PreviousNavigate());
            LoadedCommand = new RelayCommand(() => CurrentViewControlFocused = true);
            ReloadCurrentViewCommand = new RelayCommand(ReloadCurrentView);
        }

        private void ReloadCurrentView()
        {
            if (CurrentViewControl.DataContext is IReloadable reloadable)
                reloadable.Reload();
        }

        private void ChangeCurrentView()
        {
            _controls.Clear();
            CurrentViewControl = API.Configuration.Current.Session == null ?     new Login() :
                API.Configuration.Current.Session.User.Role == "manager" ?    new MainManager() : 
                                                                                new MainFranchisee();
        }

        private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Session")
            {
                ChangeCurrentView();
            }
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
