using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PortalDoFranqueadoAPIAWS.Models;
using PortalDoFranqueadoAPIAWS.Repositories;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPIAWS.Controllers
{
    [Route("api/purchase")]
    [ApiController]
    public class PurchaseController : Controller
    {
        private readonly MySqlConnection _connection;

        public PurchaseController(MySqlConnection connection)
            => _connection = connection;

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] Purchase purchase)
        {
            try
            {
                await PurchaseRepository.Save(_connection, purchase);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collection/{collectionId}/{storeId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int collectionId, int storeId)
        {
            try
            {
                var purchase = await PurchaseRepository.Get(_connection, collectionId, storeId);

                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collection/{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetPurchases(int collectionId)
        {
            try
            {
                var purchases = await PurchaseRepository.GetPurchases(_connection, collectionId);

                return Ok(purchases);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("id/{purchaseId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int purchaseId)
        {
            try
            {
                var purchase = await PurchaseRepository.Get(_connection, purchaseId);

                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("reverse")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<dynamic>> Reverse([FromBody] int purchaseId)
        {
            try
            {
                await PurchaseRepository.Reverse(_connection, purchaseId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
