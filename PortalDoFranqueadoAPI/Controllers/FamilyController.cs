using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/family")]
    [ApiController]
    public class FamilyController(IFamilyRepository familyRepository) : ControllerBase, IDisposable
    {
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
            var families = await familyRepository.GetList(withSizes).AsNoContext();
            return Ok(families);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~FamilyController() => Dispose();
    }
}