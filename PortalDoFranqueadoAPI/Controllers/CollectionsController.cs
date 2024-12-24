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
    public class CollectionsController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public CollectionsController(SqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("noclosed")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetNoClosed()
        {
            try
            {
                var collections = await CollectionRepository.GetList(_connection).AsNoContext();

                return Ok(collections);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetAll()
        {
            try
            {
                var collections = await CollectionRepository.GetList(_connection, false).AsNoContext();

                return Ok(collections);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            try
            {
                var collections = await CollectionRepository.Get(_connection, id).AsNoContext();

                return Ok(collections);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpGet]
        [Route("opened")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetOpened()
        {
            try
            {
                var collection = await CollectionRepository.GetOpenedCollection(_connection).AsNoContext();

                return Ok(collection);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Collection collection)
        {
            try
            {
                var id = await CollectionRepository.Insert(_connection, collection).AsNoContext();
                
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            try
            {
                var sucess = await CollectionRepository.Delete(_connection, id).AsNoContext();

                return Ok(sucess);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPut]
        [Route("changestatus/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateStatus(int id, [FromBody] int status)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Collection collection)
        {
            try
            {
                await CollectionRepository.Update(_connection, collection).AsNoContext();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~CollectionsController() => Dispose();
    }
}
