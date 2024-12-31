namespace PortalDoFranqueado.Model.Entities
{
    public class ProductSize
    {
        public string Size { get; set; }
        public short Order { get; set; }

        public override string ToString() => Size;
    }
}
