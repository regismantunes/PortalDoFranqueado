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
    [Route("api/store")]
    [ApiController]
    public class StoreController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetStores()
        {
            var stores = await StoreRepository.GetList(_connection).AsNoContext();
            return Ok(stores);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var store = await StoreRepository.Get(_connection, id).AsNoContext();
            return Ok(store);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Store store)
        {
            var id = await StoreRepository.Insert(_connection, store).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await StoreRepository.Delete(_connection, id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Store store)
        {
            await StoreRepository.Update(_connection, store).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~StoreController() => Dispose();
    }
}