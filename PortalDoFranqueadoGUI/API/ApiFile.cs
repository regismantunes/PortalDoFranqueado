using PortalDoFranqueado.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalDoFranqueado.Api
{
    public static class ApiFile
    {
        public static async Task<IEnumerable<MyFile>> GetFromAuxiliary(int id)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<MyFile>>($"files/auxiliary/{id}")
                            .Get();

        public static async Task<IEnumerable<MyFile>> GetFromCampaign(int id)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<MyFile>>($"files/campaign/{id}")
                            .Get();

        public static async Task<IEnumerable<MyFile>> GetFromCollection(int id)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<MyFile>>($"files/collection/{id}")
                            .Get();

        public static async Task<IEnumerable<int>> InsertAuxiliaryFiles(int id, IEnumerable<MyFile> files)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<int>>($"files/auxiliary/{id}")
                            .Post(files);

        public static async Task<IEnumerable<int>> InsertCampaignFiles(int id, IEnumerable<MyFile> files)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<int>>($"files/campaign/{id}")
                            .Post(files);

        public static async Task<IEnumerable<int>> InsertCollectionFiles(int id, IEnumerable<MyFile> files)
            => await BaseApi.GetSimpleHttpClientRequest<IEnumerable<int>>($"files/collection/{id}")
                            .Post(files);

        public static async Task<int> Insert(MyFile file)
            => await BaseApi.GetSimpleHttpClientRequest<int>("files")
                            .Post(file);

        public static async Task<MyFile> Get(int id)
            => await BaseApi.GetSimpleHttpClientRequest<MyFile>($"files/{id}")
                            .Get();

        public static async Task UploadFile(MyFile file, byte[] bytes)
            => await BaseApi.GetSimpleHttpClientRequest($"files/upload/{file.Id}/{file.CompressionType}")
                            .PostFile(bytes, file.ContentType, file.Name, string.Concat(file.Name, file.Extension));

        public static async Task<string> DownloadFile(MyFile file)
            => await BaseApi.GetSimpleHttpClientRequest($"files/download/{file.Id}")
                            .GetFile();

        public static async Task Delete(IEnumerable<int> ids)
            => await BaseApi.GetSimpleHttpClientRequest("files")
                            .Delete(ids);

        public static async Task Delete(int id)
            => await BaseApi.GetSimpleHttpClientRequest($"files/{id}")
                            .Delete();
    }
}