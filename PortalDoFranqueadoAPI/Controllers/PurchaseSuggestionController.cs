using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/purchasesuggestion")]
    [ApiController]
    public class PurchaseSuggestionController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public PurchaseSuggestionController(SqlConnection connection)
            => _connection = connection;

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Save([FromBody] PurchaseSuggestion purchaseSuggestion)
        {
            try
            {
                var id = await PurchaseSuggestionRepository.Save(_connection, purchaseSuggestion);

                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("purchaseid/{purchaseid}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetByPurchaseId(int purchaseid)
        {
            try
            {
                var purchaseSuggestion = await PurchaseSuggestionRepository.GetByPurchaseId(_connection, purchaseid);

                return Ok(purchaseSuggestion);
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

        ~PurchaseSuggestionController() => Dispose();
    }
}
