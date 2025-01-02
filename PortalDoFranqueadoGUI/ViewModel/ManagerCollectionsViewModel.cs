using CommunityToolkit.Mvvm.Input;
using PortalDoFranqueado.Model.Entities;
using PortalDoFranqueado.Model.Enums;
using PortalDoFranqueado.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PortalDoFranqueado.ViewModel
{
    internal class ManagerCollectionsViewModel : BaseViewModel, IReloadable
    {
        public class CollectionViewModel
        {
            public Collection Collection { get; set; }
            public string CurrentStatusAction { get; set; }
            public RelayCommand<Collection> CurrentStatusCommand { get; set; }
            public string NextStatusAction { get; set; }
            public bool EnabledNextStatus { get; set; }
            public RelayCommand<Collection>? NextStatusCommand { get; set; }
            public string PreviusStatusAction { get; set; }
            public bool EnabledPreviusStatus { get; set; }
            public RelayCommand<Collection>? PreviusStatusCommand { get; set; }
            public RelayCommand<Collection>? DeleteCommand { get; set; }
            public RelayCommand<Collection>? ViewPurchasesCommand { get; set; }
        }

        public class NewCollection
        {
            public FieldViewModel<DateTime?> StartDate { get; } = new FieldViewModel<DateTime?>();
            public FieldViewModel<DateTime?> EndDate { get; } = new FieldViewModel<DateTime?>();
        }

        private bool _showClosed;
        
        public NewCollection CollectionToAdd { get; }

        public ObservableCollection<CollectionViewModel> FilteredCollection { get; }
        private readonly List<Collection> _collections;
        public bool ShowClosed
        { 
            get => _showClosed;
            set 
            { 
                _showClosed = value;
                OnPropertyChanged();
                LoadCollections();
            }
        }

        public RelayCommand AddCollectionCommand { get; }
        public RelayCommand LoadedCommand { get; }
        public RelayCommand<Collection> ListViewEditCommand { get; }
        public RelayCommand<Collection> ListViewViewCommand { get; }
        public RelayCommand<Collection> ListViewOpenCommand { get; }
        public RelayCommand<Collection> ListViewCloseCommand { get; }
        public RelayCommand<Collection> ListViewReverseCommand { get; }
        public RelayCommand<Collection> ListViewDeleteCommand { get; }
        public RelayCommand<Collection> ViewPurchasesCommand { get; }

        public ManagerCollectionsViewModel()
        {
            _collections = [];
            FilteredCollection = [];
            CollectionToAdd = new NewCollection();

            AddCollectionCommand = new RelayCommand(AddCollection);
            LoadedCommand = new RelayCommand(async() => await LoadCollections());
            ListViewEditCommand = new RelayCommand<Collection>(EditCollection);
            ListViewViewCommand = new RelayCommand<Collection>(ViewCollection);
            ListViewOpenCommand = new RelayCommand<Collection>(OpenCollection);
            ListViewCloseCommand = new RelayCommand<Collection>(CloseCollection);
            ListViewReverseCommand = new RelayCommand<Collection>(ReverseCollection);
            ListViewDeleteCommand = new RelayCommand<Collection>(DeleteCollection);
            ViewPurchasesCommand = new RelayCommand<Collection>(ViewPurchases);
        }

        private void ViewPurchases(Collection collection)
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerCollectionPurchases(collection));
            }
            finally
            {
                EnableContent();
            }
        }

        private void EditCollection(Collection collection)
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerCollection(collection, true));
            }
            finally
            {
                EnableContent();
            }
        }

        private void ViewCollection(Collection collection)
        {
            try
            {
                DesableContent();

                Navigator.NavigateTo(new ManagerCollection(collection, false));
            }
            finally
            {
                EnableContent();
            }
        }

        private void OpenCollection(Collection collection)
            => ChangeStatus(collection, CollectionStatus.Opened);

        private async void ChangeStatus(Collection collection, CollectionStatus status)
        {
            try
            {
                DesableContent();

                await Api.ApiCollection.ChangeStatus(collection.Id, status);

                collection.Status = status;
                LoadFilteredCollections();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao alterar situação", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private void CloseCollection(Collection collection)
            => ChangeStatus(collection, CollectionStatus.Closed);

        private void ReverseCollection(Collection collection)
            => ChangeStatus(collection, CollectionStatus.Pendding);

        private async void DeleteCollection(Collection collection)
        {
            try
            {
                DesableContent();

                if(MessageBox.Show(Me,"Deseja realmente excluir?", "BROTHERS - Excluir", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    await Api.ApiCollection.Delete(collection.Id);
                    _collections.Remove(collection);
                    LoadFilteredCollections();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }
        }

        private async void AddCollection()
        {
            try
            {
                const string messageCaption = "BROTHERS - Falta informação";
                DesableContent();

                if(CollectionToAdd.StartDate.Value == null)
                {
                    MessageBox.Show(Me,"Informe a data de início!", messageCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                    CollectionToAdd.StartDate.IsFocused = true;
                }
                else if (CollectionToAdd.EndDate.Value == null)
                {
                    MessageBox.Show(Me,"Informe a data final!", messageCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                    CollectionToAdd.EndDate.IsFocused = true;
                }
                else
                {
                    var newCollection = new Collection
                    {
                        StartDate = (DateTime)CollectionToAdd.StartDate.Value,
                        EndDate = (DateTime)CollectionToAdd.EndDate.Value,
                        Status = CollectionStatus.Pendding
                    };

                    var newId = await Api.ApiCollection.Insert(newCollection);

                    newCollection.Id = newId;

                    _collections.Add(newCollection);
                    LoadFilteredCollections();

                    CollectionToAdd.StartDate.Value = null;
                    CollectionToAdd.EndDate.Value = null;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(Me,ex.Message, "BROTHERS - Falha ao adicionar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EnableContent();
            }   
        }

        private void LoadFilteredCollections()
        {
            var ordered = _collections.OrderBy(col => col.StartDate);
            FilteredCollection.Clear();
            foreach (var collection in ordered)
                FilteredCollection.Add(new CollectionViewModel
                {
                    Collection = collection,
                    CurrentStatusAction = collection.Status switch
                    {
                        CollectionStatus.Closed => "Visualizar",
                        _ => "Editar"
                    },
                    CurrentStatusCommand = collection.Status switch
                    {
                        CollectionStatus.Closed => ListViewViewCommand,
                        _ => ListViewEditCommand
                    },
                    NextStatusAction = collection.Status switch
                    {
                        CollectionStatus.Pendding => "Abrir",
                        CollectionStatus.Opened => "Encerrar",
                        _ => string.Empty
                    },
                    EnabledNextStatus = collection.Status != CollectionStatus.Closed,
                    NextStatusCommand = collection.Status switch
                    {
                        CollectionStatus.Pendding => ListViewOpenCommand,
                        CollectionStatus.Opened => ListViewCloseCommand,
                        _ => null
                    },
                    PreviusStatusAction = collection.Status switch
                    {
                        CollectionStatus.Opened => "Estornar",
                        CollectionStatus.Closed => "Reabrir",
                        _ => string.Empty
                    },
                    EnabledPreviusStatus = collection.Status != CollectionStatus.Pendding,
                    PreviusStatusCommand = collection.Status switch
                    {
                        CollectionStatus.Opened => ListViewReverseCommand,
                        CollectionStatus.Closed => ListViewOpenCommand,
                        _ => null
                    },
                    DeleteCommand = ListViewDeleteCommand,
                    ViewPurchasesCommand = ViewPurchasesCommand
                });
        }

        public async void Reload() => await LoadCollections();

        private async Task LoadCollections()
        {
            try
            {
                DesableContent();

                _collections.Clear();
                _collections.AddRange(await (!_showClosed ? Api.ApiCollection.GetNoClosed() : Api.ApiCollection.GetAll()));

                LoadFilteredCollections();
            }
            finally
            {
                EnableContent();
            }
        }
    }
}
