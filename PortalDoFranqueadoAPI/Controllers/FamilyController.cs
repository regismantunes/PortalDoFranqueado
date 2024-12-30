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
    public class FamilyController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

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
            var families = await FamilyRepository.GetList(_connection, withSizes).AsNoContext();
            return Ok(families);
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FamilyController() => Dispose();
    }
}