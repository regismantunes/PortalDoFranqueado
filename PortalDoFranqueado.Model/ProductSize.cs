namespace PortalDoFranqueado.Model
{
    public class ProductSize
    {
        public string Size { get; set; }
        public short Order { get; set; }

        public override string ToString() => Size;
    }
}
