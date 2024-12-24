using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/family")]
    [ApiController]
    public class FamilyController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public FamilyController(SqlConnection connection)
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
                var families = await FamilyRepository.GetList(_connection, withSizes).AsNoContext();

                return Ok(families);
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

        ~FamilyController() => Dispose();
    }
}
