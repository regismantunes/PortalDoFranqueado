using System;

namespace PortalDoFranqueadoGUI.Model
{
    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string Size { get; set; }
        public int? Quantity { get; set; }

        public int GetValueToOrder()
        {
            int? value = Size switch
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
                var i = Size.IndexOf('/');
                if (i == -1)
                    i = Size.Length;

                value = int.TryParse(Size.AsSpan(0, i), out var val) ? val : 0;
            }

            return value.Value;
        }
    }
}
