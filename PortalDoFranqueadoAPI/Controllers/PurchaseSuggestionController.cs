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
    [Route("api/purchasesuggestion")]
    [ApiController]
    public class PurchaseSuggestionController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] PurchaseSuggestion purchaseSuggestion)
        {
            var id = await PurchaseSuggestionRepository.Save(_connection, purchaseSuggestion).AsNoContext();
            return Ok(id);
        }

        [HttpGet]
        [Route("purchaseid/{purchaseid}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetByPurchaseId(int purchaseid)
        {
            var purchaseSuggestion = await PurchaseSuggestionRepository.GetByPurchaseId(_connection, purchaseid).AsNoContext();
            return Ok(purchaseSuggestion);
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~PurchaseSuggestionController() => Dispose();
    }
}