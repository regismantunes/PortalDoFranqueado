﻿using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class Product
    {
        public int? Id { get; set; }
        public int FileId { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? FamilyId { get; set; }
        public Family? Family { get; set; }
        public IEnumerable<string>? LockedSizes { get; set; }
        public ImageInfo? ImageInformation { get; set; }
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }
}