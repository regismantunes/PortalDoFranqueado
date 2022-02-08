using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using PortalDoFranqueadoAPIAWS.Models;
using PortalDoFranqueadoAPIAWS.Repositories;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PortalDoFranqueadoAPIAWS.Controllers
{
    [Route("api/main")]
    [ApiController]
    public class MainScreenController : ControllerBase
    {
        private readonly MySqlConnection _connection;
        private readonly IConfiguration _configuration;

        public MainScreenController(MySqlConnection connection, IConfiguration configuration)
            => (_connection, _configuration) = (connection, configuration);

        [HttpGet]
        [Route("info")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetMainInformations()
        {
            try
            {
                var informative = await InformativeRepository.Get(_connection);
                
                var infoCompras = await CollectionRepository.GetInfo(_connection);

                var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var googleDriveClientSecret = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "client_secret.json"));
                var googleDriveServiceCredentials = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
                var googleDriveApplicationName = _configuration["AppSettings:GoogleDriveApplication"];
                var photosFolderId = _configuration["AppSettings:FotosFolderId"];
                var supportFolderId = _configuration["AppSettings:ApoioFolderId"];

                var campaigns = await MarketingCampaignRepository.GetActiveMarketingCampaigns(_connection);

                var stores = await StoreRepository.GetStoresByUser(_connection, int.Parse(User.Identity.Name));

                return new
                {
                    InformativeTitle = informative.Title,
                    InformativeText = informative.Text,
                    EnabledPurchase = infoCompras.EnabledPurchase,
                    TextPurchase = infoCompras.TextPurchase,
                    GoogleDriveClientSecret = googleDriveClientSecret,
                    GoogleDriveServiceCredentials = googleDriveServiceCredentials,
                    GoogleDriveApplicationName = googleDriveApplicationName,
                    PhotosFolderId = photosFolderId,
                    SupportFolderId = supportFolderId,
                    Campaigns = campaigns,
                    Stores = stores
                };
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

                var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var googleDriveClientSecret = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "client_secret.json"));
                var googleDriveServiceCredentials = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
                var googleDriveApplicationName = _configuration["AppSettings:GoogleDriveApplication"];
                var photosFolderId = _configuration["AppSettings:FotosFolderId"];
                var supportFolderId = _configuration["AppSettings:ApoioFolderId"];

                return new
                {
                    InformativeTitle = informative.Title,
                    InformativeText = informative.Text,
                    GoogleDriveClientSecret = googleDriveClientSecret,
                    GoogleDriveServiceCredentials = googleDriveServiceCredentials,
                    GoogleDriveApplicationName = googleDriveApplicationName,
                    PhotosFolderId = photosFolderId,
                    SupportFolderId = supportFolderId
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
        [Route("googledrive")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetGoogleDriveScrets()
        {
            try
            {
                var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var googleDriveClientSecret = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "client_secret.json"));
                var googleDriveServiceCredentials = System.IO.File.ReadAllText(Path.Combine(currentDirectory, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user"));
                var googleDriveApplicationName = _configuration["AppSettings:GoogleDriveApplication"];

                return new
                {
                    GoogleDriveClientSecret = googleDriveClientSecret,
                    GoogleDriveServiceCredentials = googleDriveServiceCredentials,
                    GoogleDriveApplicationNa = googleDriveApplicationName
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("folderid/{folderType}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFolderId(string folderType)
        {
            try
            {
                var folderTypeName = string.Concat(folderType[0].ToString().ToUpper(), folderType[1..].ToLower());
                var folderId = _configuration[$"AppSettings:{folderTypeName}FolderId"];

                return Ok(folderId);
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
