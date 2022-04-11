﻿namespace PortalDoFranqueado.Model
{
    public class Product
    {
        public int? Id { get; set; }
        public int FileId { get; set; }
        public decimal? Price { get; set; }
        public int? FamilyId { get; set; }
        public Family? Family { get; set; }
        public string[]? LockedSizes { get; set; }
        public ImageInfo? ImageInformation { get; set; }
    }
}