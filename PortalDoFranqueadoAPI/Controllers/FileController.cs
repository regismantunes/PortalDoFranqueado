using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class FileController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public FileController(SqlConnection connection)
            => _connection = connection;

        ~FileController()
            => _connection.Dispose();

        [HttpGet]
        [Route("auxiliary/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAuxiliaryFiles(int id)
        {
            try
            {
                var files = await FileRepository.GetFilesFromAuxiliary(_connection, id);

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("auxiliary/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertAuxiliaryFiles(int id, [FromBody] MyFile[] files)
        {
            try
            {
                var ids = await FileRepository.InsertFilesToAuxiliary(_connection, id, files);

                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("campaign/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaignFiles(int id)
        {
            try
            {
                var files = await FileRepository.GetFilesFromCampaign(_connection, id);

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collection/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCollectionFiles(int id)
        {
            try
            {
                var files = await FileRepository.GetFilesFromCollection(_connection, id);

                return Ok(files);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("campaign/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCampaignFiles(int id, [FromBody] MyFile[] files)
        {
            try
            {
                var ids = await FileRepository.InsertFilesToCampaign(_connection, id, files);

                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("collection/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> InsertCollectionFiles(int id, [FromBody] MyFile[] files)
        {
            try
            {
                var ids = await FileRepository.InsertFilesToCollection(_connection, id, files);

                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Insert([FromBody] MyFile file)
        {
            try
            {
                var id = await FileRepository.Insert(_connection, file);

                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFile(int id)
        {
            try
            {
                var files = await FileRepository.GetFiles(_connection, new int[] { id });

                return Ok(files[0]);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("upload/{id}/{compressionType}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> UploadFile(int id, string compressionType)//[FromForm] IFormFile file
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                var file = files[0];

                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                
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
                        await Task.Delay(1000);
                }

                var sb64 = Convert.ToBase64String(allBytes);
                
                FileRepository.SaveFile(_connection, id, compressionType, file.ContentType, sb64);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("download/{id}")]
        [Authorize]
        public ActionResult<dynamic> DownloadFile(int id)
        {
            try
            {
                var (contentType, content) = FileRepository.GetFileContent(_connection, id);

                var bytes = Convert.FromBase64String(content);

                return File(bytes, contentType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFiles([FromBody] int[] filesId)
        {
            try
            {
                await FileRepository.DeleteFiles(_connection, filesId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> DeleteFile(int id)
        {
            try
            {
                await FileRepository.DeleteFile(_connection, id);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}