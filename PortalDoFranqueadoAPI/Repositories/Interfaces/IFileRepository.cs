using Microsoft.Data.SqlClient;
using PortalDoFranqueadoAPI.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Repositories.Interfaces
{
    public interface IFileRepository
    {
        Task<MyFile> GetFile(int id);

        Task<IEnumerable<MyFile>> GetFiles(int[] ids);

        Task<IEnumerable<MyFile>> GetFilesFromCampaign(int id);

        Task<IEnumerable<MyFile>> GetFilesFromCollection(int id);

        Task<IEnumerable<MyFile>> GetFilesFromAuxiliary(int id);

        Task<int> Insert(MyFile file);

        Task<IEnumerable<int>> InsertFilesToAuxiliary(int id, IEnumerable<MyFile> files);

        Task<IEnumerable<int>> InsertFilesToCampaign(int id, IEnumerable<MyFile> files);

        Task<IEnumerable<int>> InsertFilesToCollection(int id, IEnumerable<MyFile> files);

        Task SaveFile(int id, string compressionType, string contentType, string content);

        Task<(string, string)> GetFileContent(int id);

        Task DeleteFile(int id, IDbTransaction? transaction = null);

        Task DeleteFiles(IEnumerable<int> ids);
    }
}