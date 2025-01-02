using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PortalDoFranqueadoAPI.Models
{
    public class Product
    {
        public int? Id { get; set; }
        public string? Description { get; set; }
        [AllowNull]
        [Range(0.01, 9999.99, ErrorMessage = "Informe um valor válido para o preço.")]
        public decimal? Price { get; set; }
        public int? FamilyId { get; set; }
        public IEnumerable<string>? LockedSizes { get; set; }
        public int FileId { get; set; }
        public int? SupplierId { get; set; }
    }
}
