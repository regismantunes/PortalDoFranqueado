using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/supplier")]
    [ApiController]
    public class SupplierController(ISupplierRepository supplierRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetSuppliers()
        {
            var suppliers = await supplierRepository.GetList(false).AsNoContext();
            return Ok(suppliers);
        }

        [HttpGet]
        [Route("actives")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetActivesSuppliers()
        {
            var suppliers = await supplierRepository.GetList(true).AsNoContext();
            return Ok(suppliers);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            var supplier = await supplierRepository.Get(id).AsNoContext();
            return Ok(supplier);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Supplier supplier)
        {
            var id = await supplierRepository.Insert(supplier).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await supplierRepository.Delete(id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Supplier supplier)
        {
            await supplierRepository.Update(supplier).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~SupplierController() => Dispose();
    }
}