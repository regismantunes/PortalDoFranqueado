using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public StoreController(SqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetStores()
        {
            try
            {
                var stores = await StoreRepository.GetList(_connection);

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

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~StoreController() => Dispose();
    }
}
