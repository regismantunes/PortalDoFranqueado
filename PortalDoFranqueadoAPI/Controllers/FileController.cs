using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("auxiliary/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAuxiliaryFiles(int id)
        {
            var files = await FileRepository.GetFilesFromAuxiliary(_connection, id).AsNoContext();
            return Ok(files);
        }

        [HttpPost]
        [Route("auxiliary/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertAuxiliaryFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await FileRepository.InsertFilesToAuxiliary(_connection, id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpGet]
        [Route("campaign/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaignFiles(int id)
        {
            var files = await FileRepository.GetFilesFromCampaign(_connection, id).AsNoContext();
            return Ok(files);
        }

        [HttpGet]
        [Route("collection/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCollectionFiles(int id)
        {
            var files = await FileRepository.GetFilesFromCollection(_connection, id).AsNoContext();
            return Ok(files);
        }

        [HttpPost]
        [Route("campaign/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCampaignFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await FileRepository.InsertFilesToCampaign(_connection, id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpPost]
        [Route("collection/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCollectionFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await FileRepository.InsertFilesToCollection(_connection, id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Insert([FromBody] MyFile file)
        {
            var id = await FileRepository.Insert(_connection, file).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFile(int id)
        {
            var files = await FileRepository.GetFiles(_connection, new int[] { id }).AsNoContext();
            return Ok(files[0]);
        }

        [HttpPost]
        [Route("upload/{id}/{compressionType}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> UploadFile(int id, string compressionType)//[FromForm] IFormFile file
        {
            var files = HttpContext.Request.Form.Files;
            var file = files[0];

            var stream = new MemoryStream();
            await file.CopyToAsync(stream).AsNoContext();

            stream.Position = 0;
            var reader = new BinaryReader(stream);
            var buffer = new byte[4096000];
            var allBytes = new byte[file.Length];

            var i = 0;
            while (i < file.Length)
            {
                var count = reader.Read(buffer, 0, buffer.Length);

                for (int j = 0; j < count; j++)
                    allBytes[i++] = buffer[j];

                if (count < buffer.Length &&
                    i < file.Length)
                    await Task.Delay(1000).AsNoContext();
            }

            var sb64 = Convert.ToBase64String(allBytes);

            FileRepository.SaveFile(_connection, id, compressionType, file.ContentType, sb64);

            return Ok();
        }

        [HttpGet]
        [Route("download/{id}")]
        [Authorize]
        public ActionResult<dynamic> DownloadFile(int id)
        {
            var (contentType, content) = FileRepository.GetFileContent(_connection, id);
            var bytes = Convert.FromBase64String(content);
            return File(bytes, contentType);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFiles([FromBody] int[] filesId)
        {
            await FileRepository.DeleteFiles(_connection, filesId).AsNoContext();
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFile(int id)
        {
            await FileRepository.DeleteFile(_connection, id).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FileController() => Dispose();
    }
}