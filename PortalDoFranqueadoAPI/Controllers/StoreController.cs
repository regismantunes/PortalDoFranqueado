using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController(IStoreRepository storeRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetStores()
        {
            var stores = await storeRepository.GetList().AsNoContext();
            return Ok(stores);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var store = await storeRepository.Get(id).AsNoContext();
            return Ok(store);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Store store)
        {
            var id = await storeRepository.Insert(store).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await storeRepository.Delete(id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Store store)
        {
            await storeRepository.Update(store).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~StoreController() => Dispose();
    }
}