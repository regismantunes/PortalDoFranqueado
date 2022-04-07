using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Model;
using PortalDoFranqueado.Repository;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PortalDoFranqueado.ViewModel
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
            private int _storeId;
            private User? _user;

            private bool _nameFocus;
            private bool _treatmentFocus;
            private bool _emailFocus;
            private bool _storeFocus;

            public int? Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public string? Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            public string? Treatment { get => _treatment; set { _treatment = value; OnPropertyChanged(); } }
            public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }
            public UserRole Role { get=> _role; set { _role = value; OnPropertyChanged(); OnPropertyChanged(nameof(EnabledComboBoxStores)); } }
            public Store? Store { get => _store; set { _store = value; OnPropertyChanged(); } }
            public int StoreId 
            { 
                get => _storeId; 
                set 
                { 
                    _storeId = value; 
                    OnPropertyChanged();
                    Store = _storeId == 0 ? null : ((LocalRepository)App.Current.Resources["Cache"]).Stores.First(store => store.Id == value);
                } 
            }

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
                    OnPropertyChanged(nameof(VisibitityEditControls));
                    OnPropertyChanged(nameof(VisibitityReadControls));
                } 
            }

            public string StartEditAction => _id == null ? "Novo" : "Editar";
            public bool EnabledSatartEdit => !_isEditing;
            public Visibility VisibitityEditControls => _isEditing ? Visibility.Visible : Visibility.Collapsed;
            public Visibility VisibitityReadControls => _isEditing ? Visibility.Collapsed : Visibility.Visible;
            public bool EnabledComboBoxStores => Role == UserRole.Franchisee;
            public RelayCommand StartEditCommand { get; }
            public RelayCommand CancelEditCommand { get; }
            public RelayCommand SaveCommand { get; }
            public RelayCommand DeleteCommand { get; }
            public RelayCommand ResetPasswordCommand { get; }
            public Window Window { get; set; }

            public event EventHandler? AfterInsert;
            public event EventHandler? AfterDelete;
            public event EventHandler? AfterUpdate;

            public UserViewModel(User user)
                : this()
            {
                _user = user;
                _id = user.Id;
                _name = user.Name;
                _treatment = user.Treatment;
                _email = user.Email;
                _role = user.Role;
                _store = user.Stores?.FirstOrDefault();
                _storeId = _store?.Id ?? 0;
            }

            public UserViewModel()
            {
                _role = UserRole.Franchisee;
                _storeId = 0;

                StartEditCommand = new RelayCommand(() => { IsEditing = true; SetFocusAt(nameof(Name)); });
                CancelEditCommand = new RelayCommand(CancelEdit);
                SaveCommand = new RelayCommand(async () => await Save().ConfigureAwait(false));
                DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false));
                ResetPasswordCommand = new RelayCommand(async () => await ResetPassword().ConfigureAwait(false));
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

            public void CancelEdit()
            {
                if (!IsEditing)
                    return;

                if (_user == null)
                {
                    Name = null;
                    Treatment = null;
                    Email = null;
                    Role = UserRole.Franchisee;
                    Store = null;
                }
                else
                {
                    Name = _user.Name;
                    Treatment = _user.Treatment;
                    Email = _user.Email;
                    Role = _user.Role;
                    Store = _user.Stores?.FirstOrDefault();
                }

                SetAllFocusOff();

                IsEditing = false;
            }

            public async Task Save()
            {
                try
                {
                    SetAllFocusOff();

                    if (string.IsNullOrEmpty(_name))
                    {
                        MessageBox.Show(Window, "Informe o nome do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Name));
                        return;
                    }

                    if (string.IsNullOrEmpty(_email))
                    {
                        MessageBox.Show(Window, "Informe o email do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Email));
                        return;
                    }

                    if (_store == null &&
                        _role == UserRole.Franchisee)
                    {
                        MessageBox.Show(Window, "Informe a loja do usuário", "BROTHERS - Erro ao salvar usuário", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Store));
                        return;
                    }

                    if (_user == null)
                    {
                        _user = new User();
                        if (_role == UserRole.Franchisee)
                            _user.Stores = new Store[] { _store };
                    }

                    _user.Name = _name;
                    _user.Treatment = _treatment;
                    _user.Email = _email;
                    _user.Role = _role;

                    if (_id == null)
                    {
                        _user.Id = await API.ApiAccount.Insert(_user);
                        _id = _user.Id;
                        CancelEdit();
                        AfterInsert?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        await API.ApiAccount.Update(_user);
                        CancelEdit();
                        AfterUpdate?.Invoke(this, EventArgs.Empty);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Window, ex.Message, "BROTHERS - Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public async Task Delete()
            {
                try
                {
                    if (MessageBox.Show(Window, "Deseja realmente excluir esse usuário?", "BROTHERS - Excluir usuário", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;

                    if (_user == null)
                    {
                        CancelEdit();
                        return;
                    }

                    if (await API.ApiAccount.Delete(_user.Id))
                        AfterDelete?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Window, ex.Message, "BROTHERS - Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public async Task ResetPassword()
            {
                try
                {
                    if (MessageBox.Show(Window, "Deseja realmente resetar a senha desse usuário?", "BROTHERS - Resetar senha", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                        return;

                    if (_user == null)
                    {
                        CancelEdit();
                        return;
                    }

                    var resetCode = await API.ApiAccount.ResetPassword(_user.Id);
                    MessageBox.Show(Window, $"A senha do usuário {_email} foi resetada!{Environment.NewLine}" +
                                    $"O código para reativação é: {resetCode}", "BRTHERS - Resetar senha", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Window, ex.Message, "BROTHERS - Falha ao resetar senha", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private UserViewModel? _selectedUser;
        private int? _selectedIndex;

        public UserRole[] Roles { get; }
        public ObservableCollection<UserViewModel> Users { get; }
        public int? SelectedIndex { get => _selectedIndex; set { _selectedIndex = value; OnPropertyChanged(); } }
        public UserViewModel? SelectedUser 
        { 
            get => _selectedUser; 
            set 
            {
                if (_selectedUser != null &&
                    _selectedUser.IsEditing)
                {
                    var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 100) };

                    void tick(object? sender, EventArgs e)
                    {
                        SelectedIndex = Users.IndexOf(_selectedUser);
                        timer.Stop();
                    }

                    timer.Tick += tick;
                    timer.Start();
                    return;
                }

                if (_selectedUser != null)
                    _selectedUser.PropertyChanged -= SelectedUser_PropertyChanged;

                _selectedUser = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartEditAction));
                OnPropertyChanged(nameof(EnabledSatartEdit));
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(EnabledDelete));

                if (_selectedUser != null)
                    _selectedUser.PropertyChanged += SelectedUser_PropertyChanged;
            } 
        }

        private void SelectedUser_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEditing")
            {
                OnPropertyChanged(nameof(EnabledSatartEdit));
                OnPropertyChanged(nameof(IsEditing));
            }
            else if (e.PropertyName == "Id")
            {
                OnPropertyChanged(nameof(StartEditAction));
                OnPropertyChanged(nameof(EnabledDelete));
            }
        }

        public string StartEditAction => SelectedUser?.Id == null ? "Novo" : "Editar";
        public bool EnabledSatartEdit => !(SelectedUser?.IsEditing ?? true);
        public bool IsEditing => SelectedUser?.IsEditing ?? false;
        public bool EnabledDelete => SelectedUser?.Id != null;

        public RelayCommand LoadedCommand { get; }

        public ManagerUsersViewModel()
        {
            Roles = new UserRole[] { UserRole.Franchisee, UserRole.Manager };
            Users = new ObservableCollection<UserViewModel>();

            LoadedCommand = new RelayCommand(async () => await LoadUsers());
        }

        public void SetWindow(Window main)
        {
            Me = main;
        }

        private async Task LoadUsers()
        {
            try
            {
                DesableContent();

                var users = await API.ApiAccount.GetUsers();
                Users.Clear();

                users.ToList()
                     .ForEach(user => Users.Add(CreateUserViewModel(user)));

                Users.Add(CreateUserViewModel());

                await ((LocalRepository)App.Current.Resources["Cache"]).LoadStores();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao carregar usuários", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private UserViewModel CreateUserViewModel(User? user = null)
        {
            var userVM = user == null ? new UserViewModel() : 
                                        new UserViewModel(user);
            userVM.Window = Me;

            userVM.AfterDelete += UserVM_AfterDelete;
            userVM.AfterInsert += UserVM_AfterInsert;

            return userVM;
        }

        private void UserVM_AfterInsert(object? sender, EventArgs e)
        {
            Users.Add(CreateUserViewModel());
            OnPropertyChanged(nameof(EnabledDelete));
        }

        private void UserVM_AfterDelete(object? sender, EventArgs e)
        {
            if (sender is UserViewModel vm)
                Users.Remove(vm);
        }

        public async void Reload() => await LoadUsers();
    }
}