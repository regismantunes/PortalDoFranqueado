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
    [Route("api/supplier")]
    [ApiController]
    public class SupplierController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetSuppliers()
        {
            var suppliers = await SupplierRepository.GetList(_connection, false).AsNoContext();
            return Ok(suppliers);
        }

        [HttpGet]
        [Route("actives")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetActivesSuppliers()
        {
            var suppliers = await SupplierRepository.GetList(_connection, true).AsNoContext();
            return Ok(suppliers);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var supplier = await StoreRepository.Get(_connection, id).AsNoContext();
            return Ok(supplier);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Supplier supplier)
        {
            var id = await SupplierRepository.Insert(_connection, supplier).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await SupplierRepository.Delete(_connection, id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Supplier supplier)
        {
            await SupplierRepository.Update(_connection, supplier).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~SupplierController() => Dispose();
    }
}