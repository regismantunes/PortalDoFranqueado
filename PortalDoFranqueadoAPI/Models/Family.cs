﻿namespace PortalDoFranqueadoAPI.Models
{
    public class Family
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ProductSize[] Sizes { get; set; }
    }
}
