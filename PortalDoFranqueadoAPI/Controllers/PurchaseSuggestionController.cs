using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Services.Interfaces;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/purchasesuggestion")]
    [ApiController]
    public class PurchaseSuggestionController(IPurchaseSuggestionRepository purchaseSuggestionRepository, IPurchaseSuggestionService purchaseSuggestionService) : ControllerBase, IDisposable
    {
        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] PurchaseSuggestion purchaseSuggestion)
        {
            await purchaseSuggestionService.Validate(purchaseSuggestion).AsNoContext();

            var id = await purchaseSuggestionRepository.Save(purchaseSuggestion).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("purchaseid/{purchaseid}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetByPurchaseId(int purchaseid)
        {
            var purchaseSuggestion = await purchaseSuggestionRepository.GetByPurchaseId(purchaseid).AsNoContext();
            return Ok(purchaseSuggestion);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~PurchaseSuggestionController() => Dispose();
    }
}