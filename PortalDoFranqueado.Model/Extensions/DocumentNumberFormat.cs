﻿using System;

namespace PortalDoFranqueado.Model.Extensions
{
    public static class DocumentNumberFormat
    {
        public static string ToCNPJFormat(this string? documentNumber)
            => string.IsNullOrEmpty(documentNumber) ? string.Empty : Convert.ToUInt64(documentNumber).ToString(@"00\.000\.000\/0000\-00");
    }
}