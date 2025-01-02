using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Enums;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/collections")]
    [ApiController]
    public class CollectionsController(ICollectionRepository collectionRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("noclosed")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetNoClosed()
        {
            var collections = await collectionRepository.GetList().AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAll()
        {
            var collections = await collectionRepository.GetList(false).AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var collections = await collectionRepository.Get(id).AsNoContext();
            return Ok(collections);
        }

        [HttpGet]
        [Route("opened")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetOpened()
        {
            var collection = await collectionRepository.GetOpenedCollection().AsNoContext();
            return Ok(collection);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Collection collection)
        {
            var id = await collectionRepository.Insert(collection).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await collectionRepository.Delete(id).AsNoContext();
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
                var hasOpened = await collectionRepository.HasOpenedCollection().AsNoContext();

                if (hasOpened)
                    return BadRequest(new { message = "Já existe um período de compras aberto." });
            }

            await collectionRepository.ChangeStatus(id, collectionStatus).AsNoContext();

            return Ok();
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Collection collection)
        {
            await collectionRepository.Update(collection).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~CollectionsController() => Dispose();
    }
}