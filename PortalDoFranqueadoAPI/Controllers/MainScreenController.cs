using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/main")]
    [ApiController]
    public class MainScreenController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public MainScreenController(SqlConnection connection, IConfiguration configuration)
            => (_connection, _configuration) = ((SqlConnection)(connection as ICloneable).Clone(), configuration);

        ~MainScreenController()
            => _connection.Dispose();

        [HttpGet]
        [Route("iscompatibleversion/{version}")]
        public ActionResult<dynamic> ClientVersionIsCompatible(string version)
        {
            try
            {
                var minCompatibleVersion = new Version("1.0.7");
                var sysVersion = new Version(version);

                return Ok(sysVersion >= minCompatibleVersion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("info")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetMainInformations()
        {
            try
            {
                var informative = await InformativeRepository.Get(_connection);
                var infoCompras = await CollectionRepository.GetInfo(_connection);

                var auxiliarySupportId = int.Parse(_configuration["AppSettings:AuxiliaryApoioId"]);
                var auxiliaryPhotoId = int.Parse(_configuration["AppSettings:AuxiliaryFotosId"]);

                var campaigns = await CampaignRepository.GetList(_connection, true);
                var stores = await StoreRepository.GetListByUser(_connection, int.Parse(User.Identity.Name));

                return Ok(new
                {
                    InformativeTitle = informative.Title,
                    InformativeText = informative.Text,
                    EnabledPurchase = infoCompras.EnabledPurchase,
                    TextPurchase = infoCompras.TextPurchase,
                    AuxiliarySupportId = auxiliarySupportId,
                    AuxiliaryPhotoId = auxiliaryPhotoId,
                    Campaigns = campaigns,
                    Stores = stores
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("info/basic")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetMainBasicInfos()
        {
            try
            {
                var informative = await InformativeRepository.Get(_connection);

                var auxiliarySupportId = int.Parse(_configuration["AppSettings:AuxiliaryApoioId"]);
                var auxiliaryPhototId = int.Parse(_configuration["AppSettings:AuxiliaryFotosId"]);

                return new
                {
                    InformativeTitle = informative.Title,
                    InformativeText = informative.Text,
                    AuxiliarySupportId = auxiliarySupportId,
                    AuxiliaryPhotoId = auxiliaryPhototId,
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("informative")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetInformative()
        {
            try
            {
                var informative = await InformativeRepository.Get(_connection);
                
                return new
                {
                    InformativeTitle = informative.Title,
                    InformativeText = informative.Text,
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("auxiliary/{folderType}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFolderId(string folderType)
        {
            try
            {
                var folderTypeName = string.Concat(folderType[0].ToString().ToUpper(), folderType[1..].ToLower());
                var auxiliaryId = int.Parse(_configuration[$"AppSettings:Auxiliary{folderTypeName}Id"]);

                return Ok(auxiliaryId);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("informative")]
        [Authorize]
        public async Task<ActionResult<dynamic>> UpdateInformative([FromBody] Informative informative)
        {
            try
            {
                await InformativeRepository.Save(_connection, informative);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
