using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/campaign")]
    [ApiController]
    public class CampaignController : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection;

        public CampaignController(SqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaigns()
        {
            try
            {
                var campaigns = await CampaignRepository.GetList(_connection);

                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Campaign campaign)
        {
            try
            {
                var id = await CampaignRepository.Insert(_connection, campaign);

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
                var sucess = await CampaignRepository.Delete(_connection, id);

                return Ok(sucess);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        [HttpPut]
        [Route("changestatus/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateStatus(int id, [FromBody] int status)
        {
            try
            {
                var campaignStatus = (CampaignStatus)status;

                await CampaignRepository.ChangeStatus(_connection, id, campaignStatus);

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

        ~CampaignController() => Dispose();
    }
}
