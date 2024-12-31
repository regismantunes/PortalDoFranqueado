using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using System.Linq;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileController(IFileRepository fileRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("auxiliary/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAuxiliaryFiles(int id)
        {
            var files = await fileRepository.GetFilesFromAuxiliary(id).AsNoContext();
            return Ok(files);
        }

        [HttpPost]
        [Route("auxiliary/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertAuxiliaryFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await fileRepository.InsertFilesToAuxiliary(id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpGet]
        [Route("campaign/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaignFiles(int id)
        {
            var files = await fileRepository.GetFilesFromCampaign(id).AsNoContext();
            return Ok(files);
        }

        [HttpGet]
        [Route("collection/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCollectionFiles(int id)
        {
            var files = await fileRepository.GetFilesFromCollection(id).AsNoContext();
            return Ok(files);
        }

        [HttpPost]
        [Route("campaign/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCampaignFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await fileRepository.InsertFilesToCampaign(id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpPost]
        [Route("collection/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCollectionFiles(int id, [FromBody] MyFile[] files)
        {
            var ids = await fileRepository.InsertFilesToCollection(id, files).AsNoContext();
            return Ok(ids);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Insert([FromBody] MyFile file)
        {
            var id = await fileRepository.Insert(file).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFile(int id)
        {
            var files = await fileRepository.GetFiles([id]).AsNoContext();
            return Ok(files.First());
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

            await fileRepository.SaveFile(id, compressionType, file.ContentType, sb64);

            return Ok();
        }

        [HttpGet]
        [Route("download/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> DownloadFile(int id)
        {
            var (contentType, content) = await fileRepository.GetFileContent(id);
            var bytes = Convert.FromBase64String(content);
            return File(bytes, contentType);
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFiles([FromBody] int[] filesId)
        {
            await fileRepository.DeleteFiles(filesId).AsNoContext();
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFile(int id)
        {
            await fileRepository.DeleteFile(id).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~FileController() => Dispose();
    }
}