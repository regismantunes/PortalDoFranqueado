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
            => (_connection, _configuration) = ((SqlConnection)(connection as ICloneable).Clone(), configuration);

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] UserInput model)
        {
            try
            {
                var resetPasswordMaxAttempts = short.Parse(_configuration["AppSettings:ResetPasswordAttempts"]);
                // Recupera o usuário
                var (user, resetPassword) = await UserRepository.GetAuthenticated(_connection, model.Username, model.Password, resetPasswordMaxAttempts);

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
                    ResetPassword = resetPassword,
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
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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

        [HttpDelete]
        [Route("users/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            try
            {
                var sucess = await UserRepository.Delete(_connection, id);

                return Ok(sucess);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("users")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Update([FromBody] User user)
        {
            try
            {
                await UserRepository.Update(_connection, user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("users/pswreset")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> ResetPassword([FromBody] int id)
        {
            try
            {
                var resetCode = Random.Shared.Next(0, 999999).ToString("000000");

                if (await UserRepository.ResetPassword(_connection, id, resetCode))
                    return Ok(resetCode);

                return BadRequest(new { message = "O usuário não foi encontrado." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Route("users/pswchange")]
        [Authorize]
        public async Task<ActionResult<dynamic>> ChangePassword([FromBody] UserChangePassword changePassword)
        {
            try
            {
                if (!User.Identity.Name.Equals(changePassword.Id.ToString()))
                    throw new Exception("O código do usuário informado deve ser igual ao código do usuário atual.");

                await UserRepository.ChangePassword(_connection, changePassword.Id, changePassword.NewPassword, changePassword.NewPasswordConfirmation, changePassword.CurrentPassword);

                return Ok();
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
        [Authorize(Roles = "Manager")]
        public string Manager() => "Gerente";*/
    }
}
