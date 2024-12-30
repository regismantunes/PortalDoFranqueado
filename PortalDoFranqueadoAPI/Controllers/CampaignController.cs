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
    [Route("api/campaign")]
    [ApiController]
    public class CampaignController(SqlConnection connection) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaigns()
        {
            var campaigns = await CampaignRepository.GetList(_connection).AsNoContext();
            return Ok(campaigns);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Campaign campaign)
        {
            var id = await CampaignRepository.Insert(_connection, campaign).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await CampaignRepository.Delete(_connection, id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("changestatus/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateStatus(int id, [FromBody] int status)
        {
            var campaignStatus = (CampaignStatus)status;
            await CampaignRepository.ChangeStatus(_connection, id, campaignStatus).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~CampaignController() => Dispose();
    }
}