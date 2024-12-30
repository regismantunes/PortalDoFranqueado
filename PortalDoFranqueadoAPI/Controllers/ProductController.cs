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
    [Route("api/product")]
    [ApiController]
    public class ProductController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetProducts(int collectionId)
        {
            var products = await ProductRepository.GetList(_connection, collectionId).AsNoContext();
            return Ok(products);
        }

        [HttpPost]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert(int collectionId, [FromBody] Product product)
        {
            var id = await ProductRepository.Insert(_connection, collectionId, product).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await ProductRepository.Delete(_connection, id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Update([FromBody] Product product)
        {
            await ProductRepository.Update(_connection, product).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~ProductController() => Dispose();
    }
}