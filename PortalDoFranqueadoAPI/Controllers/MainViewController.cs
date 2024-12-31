using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Models;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/main")]
    [ApiController]
    public class MainViewController(SqlConnection connection, IConfiguration configuration, IInformativeRepository informativeRepository, ICollectionRepository collectionRepository, ICampaignRepository campaignRepository, IStoreRepository storeRepository) : ControllerBase, IDisposable
    {
        [HttpGet]
        [Route("validateconnection/{version}")]
        public async Task<ActionResult<dynamic>> ValidateConnection(string version)
        {
            try
            {
                var isServiceAvalible = await DatabaseConnectionIsAvalible();

                var minCompatibleVersion = new Version("1.0.7");
                var sysVersion = new Version(version);

                return Ok(new
                {
                    IsCompatibleVersion = sysVersion >= minCompatibleVersion,
                    IsServiceAvalible = isServiceAvalible
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<bool> DatabaseConnectionIsAvalible()
        {
            try
            {
                await connection.OpenAsync().AsNoContext();
                await connection.CloseAsync().AsNoContext();
                return true;
            }
            catch 
            {
                return false;
            }
        }

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
                var informative = await informativeRepository.Get().AsNoContext();
                var infoCompras = await collectionRepository.GetInfo().AsNoContext();

                var auxiliarySupportId = int.Parse(configuration["AppSettings:AuxiliaryApoioId"]);
                var auxiliaryPhotoId = int.Parse(configuration["AppSettings:AuxiliaryFotosId"]);

                var campaigns = await campaignRepository.GetList(true).AsNoContext();
                var stores = await storeRepository.GetListByUser(int.Parse(User.Identity.Name)).AsNoContext();

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
                var informative = await informativeRepository.Get().AsNoContext();

                var auxiliarySupportId = int.Parse(configuration["AppSettings:AuxiliaryApoioId"]);
                var auxiliaryPhototId = int.Parse(configuration["AppSettings:AuxiliaryFotosId"]);

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
                var informative = await informativeRepository.Get().AsNoContext();
                
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
                var auxiliaryId = int.Parse(configuration[$"AppSettings:Auxiliary{folderTypeName}Id"]);

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
                await informativeRepository.Save(informative).AsNoContext();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~MainViewController() => Dispose();
    }
}
