using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PortalDoFranqueadoAPICore.Repositories;
using System;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPICore.Controllers
{
    [Route("api/family")]
    [ApiController]
    public class FamilyController : Controller
    {
        private readonly MySqlConnection _connection;

        public FamilyController(MySqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFamilies()
            => await GetFamilies(false);

        [HttpGet]
        [Route("all/withsizes")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFamiliesWithSizes()
            => await GetFamilies(true);

        private async Task<ActionResult<dynamic>> GetFamilies(bool withSizes)
        {
            try
            {
                var families = await FamilyRepository.GetFamilies(_connection, withSizes);

                return Ok(families);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
