using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PortalDoFranqueadoAPI.Repositories;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController : Controller
    {
        private readonly MySqlConnection _connection;

        public StoreController(MySqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetStores()
        {
            try
            {
                var stores = await StoreRepository.GetStores(_connection);

                return Ok(stores);
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
                var store = await StoreRepository.Get(_connection, id);

                return Ok(store);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
