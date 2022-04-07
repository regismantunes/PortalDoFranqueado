using System.ComponentModel;
using System.Windows.Data;

namespace PortalDoFranqueado.Util
{
    public class PropertyGroupDescriptionPublicChange : PropertyGroupDescription
    {
        public PropertyGroupDescriptionPublicChange()
            : base() { }

        public PropertyGroupDescriptionPublicChange(string propertyName)
            : base(propertyName) { }

        public void CallPropertyChange(PropertyChangedEventArgs e) 
            => base.OnPropertyChanged(e);

        public void CallPropertyChange(string propertyName)
            => base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }
}
