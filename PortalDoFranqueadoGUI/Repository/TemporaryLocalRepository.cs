using PortalDoFranqueado.Model;
using PortalDoFranqueado.ViewModel;
using PortalDoFranqueado.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PortalDoFranqueado.Repository
{
    public class TemporaryLocalRepository : BaseNotifyPropertyChanged
    {
        private IReadOnlyList<Store> _stores = Array.Empty<Store>();
        public IReadOnlyList<Store> Stores 
        { 
            get => _stores;
            set 
            { 
                _stores = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<Family>? Families { get; private set; }

        public async Task<IReadOnlyList<Family>> LoadFamilies()
        {
            if (Families == null)
            {
                Families = await ApiFamily.GetFamilies(true);
                OnPropertyChanged(nameof(Families));
            }

            return Families;
        }

        public async Task<IReadOnlyList<Store>> LoadStores(bool reload = false)
        {
            if (Stores.Count == 0 || reload)
            {
                Stores = await ApiStore.GetStores();
                OnPropertyChanged(nameof(Stores));
            }

            return Stores;
        }
    }
}
