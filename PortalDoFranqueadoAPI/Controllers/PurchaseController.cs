using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Validations.Interfaces;
using PortalDoFranqueadoAPI.Models.Validations;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/purchase")]
    [ApiController]
    public class PurchaseController(IPurchaseRepository purchaseRepository, IPurchaseValidation purchaseValidation) : ControllerBase, IDisposable
    {
        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] Purchase purchase)
        {
            await purchaseValidation.Validate(purchase).AsNoContext();

            var id = await purchaseRepository.Save(purchase).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("collection/{collectionId}/{storeId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int collectionId, int storeId)
        {
            var purchase = await purchaseRepository.Get(collectionId, storeId).AsNoContext();
            return Ok(purchase);
        }

        [HttpGet]
        [Route("collection/{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetPurchases(int collectionId)
        {
            var purchases = await purchaseRepository.GetList(collectionId).AsNoContext();
            return Ok(purchases);
        }

        [HttpGet]
        [Route("id/{purchaseId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int purchaseId)
        {
            var purchase = await purchaseRepository.Get(purchaseId).AsNoContext();
            return Ok(purchase);
        }

        [HttpPut]
        [Route("reverse")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Reverse([FromBody] int purchaseId)
        {
            await purchaseRepository.Reverse(purchaseId).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~PurchaseController() => Dispose();
    }
}