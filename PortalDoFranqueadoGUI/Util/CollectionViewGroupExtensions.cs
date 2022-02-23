using System.Windows.Data;

namespace PortalDoFranqueadoGUI.Util
{
    public static class CollectionViewGroupExtensions 
    {
        public static bool GetIsExpanded(this CollectionViewGroup group)
        {
            if (group.ItemCount == 0)
                return false;

            if(group.Items[0] is IExpandable expandable)
                return expandable.IsExpanded;

            return false;
        }

        public static void SetIsExpanded(this CollectionViewGroup group, bool value)
        {
            if (group.ItemCount == 0)
                return;

            if (group.Items[0] is IExpandable expandable)
                expandable.IsExpanded = value;
        }
    }
}
