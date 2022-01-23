using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly MySqlConnection _connection;
        /*private readonly ILogger<ProductController> _logger;

        public ProductController(MySqlConnection connection, ILogger<ProductController> logger)
            => (_connection, _logger) = (connection, logger);*/

        public ProductController(MySqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetProducts(int collectionId)
        {
            try
            {
                var products = await ProductRepository.GetProducts(_connection, collectionId);

                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("{collectionId}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert(int collectionId, [FromBody] Product product)
        {
            try
            {
                var id = await ProductRepository.Insert(_connection, collectionId, product);

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
                var sucess = await ProductRepository.Delete(_connection, id);

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
        public async Task<ActionResult<dynamic>> Update([FromBody] Product product)
        {
            try
            {
                await ProductRepository.Update(_connection, product);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
