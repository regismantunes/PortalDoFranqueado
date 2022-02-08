using System;

namespace PortalDoFranqueadoAPIAWS.Models.Validations
{
    public static class ProductValidation
    {
        public static void Validate(this Product product)
        {
            if (product.Price.HasValue &&
                (product.Price < (decimal)0.01 || product.Price.Value > (decimal)9999.99))
                throw new Exception("Informe um valor válido para o preço.");
        }
    }
}
