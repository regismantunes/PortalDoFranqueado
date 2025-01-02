using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Entities.Extensions;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerStoresViewModel : BaseViewModel, IReloadable
    {
        public class StoreViewModel : BaseNotifyPropertyChanged
        {
            private int? _id;
            private string? _name;
            private string? _documentNumber;
            private Store? _store;

            private bool _nameFocus;
            private bool _documentNumberFocus;

            public int? Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public string? Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            public string? DocumentNumber { get => _documentNumber; set { _documentNumber = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormatedDocumentNumber)); } }
            public string? FormatedDocumentNumber => _documentNumber?.ToCnpjFormat();

            public bool NameFocus { get => _nameFocus; private set { _nameFocus = value; OnPropertyChanged(); } }
            public bool DocumentNumberFocus { get => _documentNumberFocus; private set { _documentNumberFocus = value; OnPropertyChanged(); } }

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
            public RelayCommand StartEditCommand { get; }
            public RelayCommand SaveCommand { get; }
            public RelayCommand DeleteCommand { get; }
            public Window Window { get; set; }

            public event EventHandler? AfterInsert;
            public event EventHandler? AfterDelete;
            public event EventHandler? AfterUpdate;

            public StoreViewModel(Store store)
                : this()
            {
                _store = store;
                _id = store.Id;
                _name = store.Name;
                _documentNumber = store.DocumentNumber;
            }

            public StoreViewModel()
            {
                StartEditCommand = new RelayCommand(StartEdit);
                SaveCommand = new RelayCommand(async () => await Save().ConfigureAwait(false));
                DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false));
            }

            public void StartEdit()
            {
                IsEditing = true;
                SetFocusAt(nameof(Name));
            }

            private void SetAllFocusOff()
            {
                NameFocus = false;
                DocumentNumberFocus = false;
            }

            private void SetFocusAt(string fieldName)
            {
                SetAllFocusOff();

                switch (fieldName)
                {
                    case nameof(Name):
                        NameFocus = true;
                        break;
                    case nameof(DocumentNumber):
                        DocumentNumberFocus = true;
                        break;
                }
            }

            public void CancelEdit()
            {
                if (!IsEditing)
                    return;

                if (_store == null)
                {
                    Name = null;
                    DocumentNumber = null;
                }
                else
                {
                    Name = _store.Name;
                    DocumentNumber = _store.DocumentNumber;
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
                        MessageBox.Show(Window, "Informe o nome da loja", "BROTHERS - Erro ao salvar loja", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(Name));
                        return;
                    }

                    if (string.IsNullOrEmpty(_documentNumber))
                    {
                        MessageBox.Show(Window, "Informe o CNPJ da loja", "BROTHERS - Erro ao salvar loja", MessageBoxButton.OK, MessageBoxImage.Error);
                        SetFocusAt(nameof(DocumentNumber));
                        return;
                    }

                    if (_store == null)
                        _store = new Store();

                    _store.Name = _name;
                    _store.DocumentNumber = _documentNumber;

                    if (_id == null)
                    {
                        _store.Id = await Api.ApiStore.Insert(_store);
                        _id = _store.Id;
                        CancelEdit();
                        AfterInsert?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        await Api.ApiStore.Update(_store);
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

                    if (_store == null)
                    {
                        CancelEdit();
                        return;
                    }

                    if (await Api.ApiStore.Delete(_store.Id))
                        AfterDelete?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Window, ex.Message, "BROTHERS - Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private StoreViewModel? _selectedStore;
        private int? _selectedIndex;

        public ObservableCollection<StoreViewModel> Stores { get; }
        public int? SelectedIndex { get => _selectedIndex; set { _selectedIndex = value; OnPropertyChanged(); } }
        public StoreViewModel? SelectedStore
        {
            get => _selectedStore;
            set
            {
                if (_selectedStore != null &&
                    _selectedStore.IsEditing)
                {
                    var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 100) };

                    void tick(object? sender, EventArgs e)
                    {
                        SelectedIndex = Stores.IndexOf(_selectedStore);
                        timer.Stop();
                    }

                    timer.Tick += tick;
                    timer.Start();
                    return;
                }

                if (_selectedStore != null)
                    _selectedStore.PropertyChanged -= SelectedStore_PropertyChanged;

                _selectedStore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartEditAction));
                OnPropertyChanged(nameof(EnabledSatartEdit));
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(EnabledDelete));

                if (_selectedStore != null)
                    _selectedStore.PropertyChanged += SelectedStore_PropertyChanged;
            }
        }

        private void SelectedStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        public string StartEditAction => SelectedStore?.Id == null ? "Novo" : "Editar";
        public bool EnabledSatartEdit => !(SelectedStore?.IsEditing ?? true);
        public bool IsEditing => SelectedStore?.IsEditing ?? false;
        public bool EnabledDelete => SelectedStore?.Id != null;

        public RelayCommand LoadedCommand { get; }
        public RelayCommand NewRecordCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public ManagerStoresViewModel()
        {
            Stores = [];

            LoadedCommand = new RelayCommand(async () => await LoadStores());
            NewRecordCommand = new RelayCommand(NewRecord);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private void CancelEdit()
        {
            if (SelectedStore == null)
                return;

            SelectedStore.CancelEdit();
            if (SelectedStore.Id == null)
                Stores.Remove(SelectedStore);
        }

        private void NewRecord()
        {
            var newStore = CreateSupplierViewModel();
            Stores.Add(newStore);
            SelectedIndex = Stores.Count - 1;
            newStore.StartEdit();
        }

        private async Task LoadStores()
        {
            try
            {
                DesableContent();

                var stores = await Api.ApiStore.GetStores();
                Stores.Clear();

                stores.ToList()
                      .ForEach(store => Stores.Add(CreateSupplierViewModel(store)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me, ex.Message, "BROTHERS - Falha ao carregar usuários", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private StoreViewModel CreateSupplierViewModel(Store? store = null)
        {
            var vm = store == null ? new StoreViewModel() :
                                     new StoreViewModel(store);
            vm.Window = Me;
            vm.AfterDelete += VM_AfterDelete;

            return vm;
        }

        private void VM_AfterDelete(object? sender, EventArgs e)
        {
            if (sender is StoreViewModel vm)
                Stores.Remove(vm);
        }

        public async void Reload() => await LoadStores();

        public override bool BeforeReturn()
        {
            if (Stores.Any(s => s.IsEditing))
                return MessageBox.Show(Me, "Existem alterações que não foram salvas, deseja continuar?", "BROTHERS - Deseja sair sem salvar?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            return true;
        }
    }
}
