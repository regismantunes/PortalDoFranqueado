using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class Family
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<ProductSize> Sizes { get; set; }
    }
}
