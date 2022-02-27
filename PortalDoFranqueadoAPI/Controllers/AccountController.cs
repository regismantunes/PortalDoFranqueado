using PortalDoFranqueadoAPI.Repositories;
using PortalDoFranqueadoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SqlConnection _connection; 
        private readonly IConfiguration _configuration;

        public AccountController(SqlConnection connection, IConfiguration configuration)
            => (_connection, _configuration) = (connection, configuration);

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] UserInput model)
        {
            try
            {
                // Recupera o usuário
                var user = await UserRepository.Get(_connection, model.Username, model.Password);

                // Verifica se o usuário existe
                if (user == null)
                    return BadRequest(new { message = "Usuário ou senha inválidos" });
                
                // Gera o Token
                var authenticateData = TokenService.GerarTokenJwt(_configuration["AppSettings:SecretToken"], user);

                // Oculta a senha
                user.Password = string.Empty;

                // Retorna os dados
                return new
                {
                    Token = authenticateData.Token,
                    Expires = authenticateData.Expires,
                    User = user
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("users/all")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<dynamic>> GetUsers()
        {
            try
            {
                var users = await UserRepository.GetList(_connection);

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Route("users")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<dynamic>> Insert([FromBody] User user)
        {
            try
            {
                var id = await UserRepository.Insert(_connection, user);

                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /*[HttpGet]
        [Route("anonymous")]
        [AllowAnonymous]
        public string Anonymous() => "Anônimo";

        [HttpGet]
        [Route("authenticated")]
        [Authorize]
        public string Authenticated() => String.Format("Autenticado - {0}", User.Identity.Name);

        [HttpGet]
        [Route("employee")]
        [Authorize(Roles = "employee,manager")]
        public string Employee() => "Funcionário";

        [HttpGet]
        [Route("manager")]
        [Authorize(Roles = "manager")]
        public string Manager() => "Gerente";*/
    }
}
