﻿using System;

namespace PortalDoFranqueadoGUI.Model
{
    public class Collection
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MyFile[] Files { get; set; }
        public CollectionStatus Status { get; set; }
    }
}
