﻿using System.Collections.Generic;

namespace PortalDoFranqueado.Model.Entities
{
    public class MainInfos
    {
        public string InformativeTitle { get; set; }
        public string InformativeText { get; set; }
        public bool EnabledPurchase { get; set; }
        public string TextPurchase { get; set; }

        public int AuxiliarySupportId { get; set; }
        public int AuxiliaryPhotoId { get; set; }

        public IEnumerable<Campaign> Campaigns { get; set; }
        public IEnumerable<Store> Stores { get; set; }
    }
}
