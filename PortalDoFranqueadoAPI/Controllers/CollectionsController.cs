using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/collections")]
    [ApiController]
    public class CollectionsController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("noclosed")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetNoClosed()
        {
            var collections = await CollectionRepository.GetList(_connection).AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAll()
        {
            var collections = await CollectionRepository.GetList(_connection, false).AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var collections = await CollectionRepository.Get(_connection, id).AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("opened")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetOpened()
        {
            var collection = await CollectionRepository.GetOpenedCollection(_connection).AsNoContext();
            return Ok(collection);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Collection collection)
        {
            var id = await CollectionRepository.Insert(_connection, collection).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await CollectionRepository.Delete(_connection, id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("changestatus/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateStatus(int id, [FromBody] int status)
        {
            var collectionStatus = (CollectionStatus)status;

            if (collectionStatus == CollectionStatus.Opened)
            {
                var hasOpened = await CollectionRepository.HasOpenedCollection(_connection).AsNoContext();

                if (hasOpened)
                    return BadRequest(new { message = "Já existe um período de compras aberto." });
            }

            await CollectionRepository.ChangeStatus(_connection, id, collectionStatus).AsNoContext();

            return Ok();
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Collection collection)
        {
            await CollectionRepository.Update(_connection, collection).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~CollectionsController() => Dispose();
    }
}