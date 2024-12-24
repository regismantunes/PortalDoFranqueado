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
    [Route("api/purchase")]
    [ApiController]
    public class PurchaseController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public PurchaseController(SqlConnection connection)
            => _connection = connection;

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] Purchase purchase)
        {
            try
            {
                var id = await PurchaseRepository.Save(_connection, purchase).AsNoContext();

                return Ok(id);
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
                var purchase = await PurchaseRepository.Get(_connection, collectionId, storeId).AsNoContext();

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
                var purchases = await PurchaseRepository.GetList(_connection, collectionId).AsNoContext();

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
                var purchase = await PurchaseRepository.Get(_connection, purchaseId).AsNoContext();

                return Ok(purchase);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPut]
        [Route("reverse")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Reverse([FromBody] int purchaseId)
        {
            try
            {
                await PurchaseRepository.Reverse(_connection, purchaseId).AsNoContext();

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

        ~PurchaseController() => Dispose();
    }
}
