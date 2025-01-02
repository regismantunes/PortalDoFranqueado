using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using System.Linq;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(IProductRepository productRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetProducts(int collectionId)
        {
            var products = await productRepository.GetList(collectionId).AsNoContext();
            return Ok(products);
        }

        [HttpPost]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert(int collectionId, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.First().Errors.First().ErrorMessage);

            var id = await productRepository.Insert(collectionId, product).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await productRepository.Delete(id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.First().Errors.First().ErrorMessage);

            await productRepository.Update(product).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~ProductController() => Dispose();
    }
}