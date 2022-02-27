using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueadoGUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueadoGUI.ViewModel
{
    internal class ManagerUsersViewModel : BaseViewModel, IReloadable
    {
        public class UserViewModel : BaseNotifyPropertyChanged
        {
            private int? _id;
            private string? _name;
            private string? _treatment;
            private string? _email;
            private UserRole _role;
            private Store? _store;
            private User _user;

            private bool _nameFocus;
            private bool _treatmentFocus;
            private bool _emailFocus;
            private bool _storeFocus;

            public int? Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public string? Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            public string? Treatment { get => _treatment; set { _treatment = value; OnPropertyChanged(); } }
            public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }
            public UserRole Role { get=> _role; set { _role = value; OnPropertyChanged(); } }
            public Store? Store { get => _store; set { _store = value; OnPropertyChanged(); } }

            public bool NameFocus { get => _nameFocus; private set { _nameFocus = value; OnPropertyChanged(); } }
            public bool TreatmentFocus { get => _treatmentFocus; private set { _treatmentFocus = value; OnPropertyChanged(); } }
            public bool EmailFocus { get => _emailFocus; private set { _emailFocus = value; OnPropertyChanged(); } }
            public bool StoreFocus { get => _storeFocus; private set { _storeFocus = value; OnPropertyChanged(); } }

            private bool _isEditing;
            public bool IsEditing 
            { 
                get => _isEditing; 
                set 
                { 
                    _isEditing = value; 
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EnabledSatartEdit));
                } 
            }

            public string StartEditAction => _id == null ? "Novo" : "Editar";
            public bool EnabledSatartEdit => !_isEditing;
            public RelayCommand StartEditCommand { get; }
            public RelayCommand SaveCommand { get; }
            public RelayCommand CancelCommand { get; }
            public RelayCommand DeleteCommand { get; }

            public UserViewModel(User user)
            {
                _user = user;
                _id = user.Id;
                _name = user.Name;
                _treatment = user.Treatment;
                _email = user.Email;
                _role = user.Role;
                if (user.Stores != null &&
                    user.Stores.Any())
                    _store = user.Stores[0];

                StartEditCommand = new RelayCommand(() => IsEditing = true);
                SaveCommand = new RelayCommand(async () => await Save());
            }

            public UserViewModel()
            {
                _role = UserRole.Franchisee;
            }

            private void SetAllFocusOff()
            {
                NameFocus = false;
                TreatmentFocus = false;
                EmailFocus = false;
                StoreFocus = false;
            }

            private void SetFocusAt(string fieldName)
            {
                SetAllFocusOff();

                switch (fieldName)
                {
                    case nameof(Name):
                        NameFocus = true;
                        break;
                    case nameof(Treatment):
                        TreatmentFocus = true;
                        break;
                    case nameof(Email):
                        EmailFocus = true;
                        break;
                    case nameof(Store):
                        StoreFocus = true;
                        break;
                }
            }

            public async Task Save()
            {
                try
                {
                    SetAllFocusOff();

                    if (string.IsNullOrEmpty(_name))
                    {
                        MessageBox.Show("Informe o nome do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Name));
                        return;
                    }

                    if (string.IsNullOrEmpty(_email))
                    {
                        MessageBox.Show("Informe o email do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Email));
                        return;
                    }

                    if (_store == null &&
                        _role == UserRole.Franchisee)
                    {
                        MessageBox.Show("Informe a loja do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Store));
                        return;
                    }

                    if (_user == null)
                        _user = new User();

                    _user.Name = _name;
                    _user.Treatment = _treatment;
                    _user.Email = _email;
                    _user.Role = _role;

                    if (_id == null)
                        _user.Id = await API.ApiAccount.Insert(_user);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "BROTHERS - Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        public UserRole[] Roles { get; }
        public ObservableCollection<UserViewModel> Users { get; }

        public RelayCommand LoadedCommand { get; }

        public ManagerUsersViewModel()
        {
            Roles = new UserRole[] { UserRole.Franchisee, UserRole.Manager };
            Users = new ObservableCollection<UserViewModel>();

            LoadedCommand = new RelayCommand(async () => await LoadUsers());
        }

        private async Task LoadUsers()
        {
            try
            {
                DesableContent();

                var users = API.ApiAccount.GetUsers();
                Users.Clear();

            }
            finally
            {
                EnableContent();
            }
        }

        public async void Reload() => await LoadUsers();
    }
}
