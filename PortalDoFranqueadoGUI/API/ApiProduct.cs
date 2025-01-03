﻿using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiProduct
    {
        public static async Task<IEnumerable<Product>> Get(int collectionId)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<Product>>($"product/{collectionId}")
                            .Get();

        public static async Task<int> Insert(int collectionId, Product product)
            => await BaseApi.GetSimpleHttpClientRequest<int>($"product/{collectionId}")
                            .Post(product);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"product/{id}")
                            .Delete();

        public static async Task Update(Product product)
            => await BaseApi.GetSimpleHttpClientRequest("product")
                            .Put(product);
    }
}
