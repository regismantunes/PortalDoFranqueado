using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Enums;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/campaign")]
    [ApiController]
    public class CampaignController(ICampaignRepository campaignRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetCampaigns()
        {
            var campaigns = await campaignRepository.GetList().AsNoContext();
            return Ok(campaigns);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Insert([FromBody] Campaign campaign)
        {
            var id = await campaignRepository.Insert(campaign).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await campaignRepository.Delete(id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("changestatus/{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateStatus(int id, [FromBody] int status)
        {
            var campaignStatus = (CampaignStatus)status;
            await campaignRepository.ChangeStatus(id, campaignStatus).AsNoContext();
            return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~CampaignController() => Dispose();
    }
}