using System;

namespace PortalDoFranqueadoGUI.Model.Order
{
    public static class OrderSize
    {
        public static int GetValue(string size)
        {
            int? value = size switch
            {
                "Único" => 0,
                "P" => 0,
                "M" => 1,
                "G" => 2,
                "GG" => 3,
                "G1" => 4,
                "G2" => 5,
                "G3" => 6,
                _ => null
            };

            if (!value.HasValue)
            {
                var i = size.IndexOf('/');
                if (i == -1)
                    i = size.Length;

                value = int.TryParse(size.AsSpan(0, i), out var val) ? val : 0;
            }

            return value.Value;
        }
    }
}
