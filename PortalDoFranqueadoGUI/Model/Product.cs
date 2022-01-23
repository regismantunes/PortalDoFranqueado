namespace PortalDoFranqueadoGUI.Model
{
    public class Product
    {
        public int? Id { get; set; }
        public string FileId { get; set; }
        public decimal? Price { get; set; }
        public int? FamilyId { get; set; }
        public Family? Family { get; set; }
    }
}
