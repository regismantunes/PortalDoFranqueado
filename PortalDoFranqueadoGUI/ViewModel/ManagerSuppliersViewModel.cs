using GalaSoft.MvvmLight.CommandWpf;
using PortalDoFranqueado.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerSuppliersViewModel : BaseViewModel, IReloadable
    {
        public class SupplierViewModel : BaseNotifyPropertyChanged
        {
            private int? _id;
            private string? _name;
            private bool _active;
            private Supplier? _supplier;

            private bool _nameFocus;
            private bool _activeFocus;

            public int? Id { get => _id; set { _id = value; OnPropertyChanged(); } }
            public string? Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            public bool Active { get => _active; set { _active = value; OnPropertyChanged(); } }

            public bool NameFocus { get => _nameFocus; private set { _nameFocus = value; OnPropertyChanged(); } }
            public bool ActiveFocus { get => _activeFocus; private set { _activeFocus = value; OnPropertyChanged(); } }
            
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

            public SupplierViewModel(Supplier supplier)
                : this()
            {
                _supplier = supplier;
                _id = supplier.Id;
                _name = supplier.Name;
                _active = supplier.Active;
            }

            public SupplierViewModel()
            {
                _active = true;

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
                ActiveFocus = false;
            }

            private void SetFocusAt(string fieldName)
            {
                SetAllFocusOff();

                switch (fieldName)
                {
                    case nameof(Name):
                        NameFocus = true;
                        break;
                    case nameof(Active):
                        ActiveFocus = true;
                        break;
                }
            }

            public void CancelEdit()
            {
                if (!IsEditing)
                    return;

                if (_supplier == null)
                {
                    Name = null;
                    Active = true;
                }
                else
                {
                    Name = _supplier.Name;
                    Active = _supplier.Active;
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

                    if (_supplier == null)
                        _supplier = new Supplier();

                    _supplier.Name = _name;
                    _supplier.Active = _active;

                    if (_id == null)
                    {
                        _supplier.Id = await Api.ApiSupplier.Insert(_supplier);
                        _id = _supplier.Id;
                        CancelEdit();
                        AfterInsert?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        await Api.ApiSupplier.Update(_supplier);
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

                    if (_supplier == null)
                    {
                        CancelEdit();
                        return;
                    }

                    if (await Api.ApiSupplier.Delete(_supplier.Id))
                        AfterDelete?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Window, ex.Message, "BROTHERS - Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private SupplierViewModel? _selectedSupplier;
        private int? _selectedIndex;

        public ObservableCollection<SupplierViewModel> Suppliers { get; }
        public int? SelectedIndex { get => _selectedIndex; set { _selectedIndex = value; OnPropertyChanged(); } }
        public SupplierViewModel? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier != null &&
                    _selectedSupplier.IsEditing)
                {
                    var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 100) };

                    void tick(object? sender, EventArgs e)
                    {
                        SelectedIndex = Suppliers.IndexOf(_selectedSupplier);
                        timer.Stop();
                    }

                    timer.Tick += tick;
                    timer.Start();
                    return;
                }

                if (_selectedSupplier != null)
                    _selectedSupplier.PropertyChanged -= SelectedSupplier_PropertyChanged;

                _selectedSupplier = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartEditAction));
                OnPropertyChanged(nameof(EnabledSatartEdit));
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(EnabledDelete));

                if (_selectedSupplier != null)
                    _selectedSupplier.PropertyChanged += SelectedSupplier_PropertyChanged;
            }
        }

        private void SelectedSupplier_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        public string StartEditAction => SelectedSupplier?.Id == null ? "Novo" : "Editar";
        public bool EnabledSatartEdit => !(SelectedSupplier?.IsEditing ?? true);
        public bool IsEditing => SelectedSupplier?.IsEditing ?? false;
        public bool EnabledDelete => SelectedSupplier?.Id != null;

        public RelayCommand LoadedCommand { get; }
        public RelayCommand NewRecordCommand { get; }
        public RelayCommand CancelEditCommand { get; }

        public ManagerSuppliersViewModel()
        {
            Suppliers = new ObservableCollection<SupplierViewModel>();

            LoadedCommand = new RelayCommand(async () => await LoadSuppliers());
            NewRecordCommand = new RelayCommand(NewRecord);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private void CancelEdit()
        {
            if (SelectedSupplier == null)
                return;

            SelectedSupplier.CancelEdit();
            if (SelectedSupplier.Id == null)
                Suppliers.Remove(SelectedSupplier);
        }

        private void NewRecord()
        {
            var newSupplier = CreateSupplierViewModel();
            Suppliers.Add(newSupplier);
            SelectedIndex = Suppliers.Count - 1;
            newSupplier.StartEdit();
        }

        private async Task LoadSuppliers()
        {
            try
            {
                DesableContent();

                var suppliers = await Api.ApiSupplier.GetSuppliers(false);
                Suppliers.Clear();

                suppliers.ToList()
                         .ForEach(supplier => Suppliers.Add(CreateSupplierViewModel(supplier)));
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

        private SupplierViewModel CreateSupplierViewModel(Supplier? supplier = null)
        {
            var vm = supplier == null ? new SupplierViewModel() :
                                        new SupplierViewModel(supplier);
            vm.Window = Me;
            vm.AfterDelete += VM_AfterDelete;
        
            return vm;
        }

        private void VM_AfterDelete(object? sender, EventArgs e)
        {
            if (sender is SupplierViewModel vm)
                Suppliers.Remove(vm);
        }

        public async void Reload() => await LoadSuppliers();

        public override bool BeforeReturn()
        {
            if (Suppliers.Any(s => s.IsEditing))
                return MessageBox.Show(Me, "Existem alterações que não foram salvas, deseja continuar?", "BROTHERS - Deseja sair sem salvar?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            return true;
        }
    }
}
