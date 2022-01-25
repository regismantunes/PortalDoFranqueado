﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/collections")]
    [ApiController]
    public class CollectionsController : Controller
    {
        private readonly MySqlConnection _connection;

        public CollectionsController(MySqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("noclosed")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetNoClosed()
        {
            try
            {
                var collections = await CollectionRepository.GetCollections(_connection);

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
                var collections = await CollectionRepository.GetCollections(_connection, false);

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
                var collection = await CollectionRepository.GetOpenedCollection(_connection);

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
                var id = await CollectionRepository.Insert(_connection, collection);
                
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
                var sucess = await CollectionRepository.Delete(_connection, id);

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
                    var hasOpened = await CollectionRepository.HasOpenedCollection(_connection);

                    if (hasOpened)
                        return BadRequest(new { message = "Já existe um período de compras aberto." });
                }

                await CollectionRepository.ChangeCollectionStatus(_connection, id, collectionStatus);

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
                await CollectionRepository.Update(_connection, collection);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}