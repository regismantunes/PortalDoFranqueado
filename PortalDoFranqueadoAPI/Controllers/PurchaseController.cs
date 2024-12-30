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
    public class PurchaseController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] Purchase purchase)
        {
            var id = await PurchaseRepository.Save(_connection, purchase).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("collection/{collectionId}/{storeId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int collectionId, int storeId)
        {
            var purchase = await PurchaseRepository.Get(_connection, collectionId, storeId).AsNoContext();
            return Ok(purchase);
        }

        [HttpGet]
        [Route("collection/{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetPurchases(int collectionId)
        {
            var purchases = await PurchaseRepository.GetList(_connection, collectionId).AsNoContext();
            return Ok(purchases);
        }

        [HttpGet]
        [Route("id/{purchaseId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int purchaseId)
        {
            var purchase = await PurchaseRepository.Get(_connection, purchaseId).AsNoContext();
            return Ok(purchase);
        }

        [HttpPut]
        [Route("reverse")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Reverse([FromBody] int purchaseId)
        {
            await PurchaseRepository.Reverse(_connection, purchaseId).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~PurchaseController() => Dispose();
    }
}