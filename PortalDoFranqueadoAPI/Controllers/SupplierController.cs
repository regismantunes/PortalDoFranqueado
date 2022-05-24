using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/supplier")]
    [ApiController]
    public class SupplierController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public SupplierController(SqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetSuppliers()
        {
            try
            {
                var suppliers = await SupplierRepository.GetList(_connection, false);

                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpGet]
        [Route("actives")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetActivesSuppliers()
        {
            try
            {
                var suppliers = await SupplierRepository.GetList(_connection, true);

                return Ok(suppliers);
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
                var supplier = await StoreRepository.Get(_connection, id);

                return Ok(supplier);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Supplier supplier)
        {
            try
            {
                var id = await SupplierRepository.Insert(_connection, supplier);

                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            try
            {
                var sucess = await SupplierRepository.Delete(_connection, id);

                return Ok(sucess);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Supplier supplier)
        {
            try
            {
                await SupplierRepository.Update(_connection, supplier);

                return Ok();
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

        ~SupplierController() => Dispose();
    }
}
