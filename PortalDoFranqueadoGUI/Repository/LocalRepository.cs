using PortalDoFranqueadoGUI.Model;
using PortalDoFranqueadoGUI.ViewModel;
using PortalDoFranqueadoGUI.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PortalDoFranqueadoGUI.Repository
{
    public class LocalRepository : BaseNotifyPropertyChanged
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
            if (Families is null)
            {
                Families = await ApiFamily.GetFamilies(true);
                OnPropertyChanged(nameof(Families));
            }

            return Families;
        }
    }
}
