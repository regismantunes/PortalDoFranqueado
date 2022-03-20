using PortalDoFranqueadoGUI.Model;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoGUI.API
{
    public static class ApiFile
    {
        public static async Task<MyFile[]> GetFromAuxiliary(int id)
            => await BaseApi.GetSimpleHttpClientRequest<MyFile[]>($"files/auxiliary/{id}")
                            .Get();

        public static async Task<MyFile[]> GetFromCampaign(int id)
            => await BaseApi.GetSimpleHttpClientRequest<MyFile[]>($"files/campaign/{id}")
                            .Get();

        public static async Task<MyFile[]> GetFromCollection(int id)
            => await BaseApi.GetSimpleHttpClientRequest<MyFile[]>($"files/collection/{id}")
                            .Get();

        public static async Task<int[]> InsertAuxiliaryFiles(int id, MyFile[] files)
            => await BaseApi.GetSimpleHttpClientRequest<int[]>($"files/auxiliary/{id}")
                            .Post(files);

        public static async Task<int[]> InsertCampaignFiles(int id, MyFile[] files)
            => await BaseApi.GetSimpleHttpClientRequest<int[]>($"files/campaign/{id}")
                            .Post(files);

        public static async Task<int[]> InsertCollectionFiles(int id, MyFile[] files)
            => await BaseApi.GetSimpleHttpClientRequest<int[]>($"files/collection/{id}")
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

        public static async Task<string> DownloadFile(int id)
            => await BaseApi.GetSimpleHttpClientRequest($"files/download/{id}")
                            .GetFile();

        public static async Task Delete(int[] ids)
            => await BaseApi.GetSimpleHttpClientRequest("files")
                            .Delete(ids);

        public static async Task Delete(int id)
            => await Delete(new int[] { id });
    }
}