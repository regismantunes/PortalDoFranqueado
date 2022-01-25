﻿using PortalDoFranqueadoGUI.Model;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiProduct
    {
        public static async Task<Product[]> Get(int collectionId)
            => await BaseApi.GetSimpleHttpClientRequest<Product[]>($"product/{collectionId}")
                            .Get();

        public static async Task<int> Insert(int collectionId, Product product)
            => await BaseApi.GetSimpleHttpClientRequest<int>($"product/{collectionId}/insert")
                            .Post(product);

        public static async Task<bool> Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest<bool>($"product/delete/{id}")
                            .Delete();

        public static async Task Update(Product product)
            => await BaseApi.GetSimpleHttpClientRequest("product/update")
                            .Put(product);
    }
}