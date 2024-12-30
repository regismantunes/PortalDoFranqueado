using PortalDoFranqueadoAPI.Repositories;
using PortalDoFranqueadoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Extensions;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(SqlConnection connection, IConfiguration configuration) : ControllerBase, IDisposable
    {
        private readonly SqlConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] UserInput model)
        {
            try
            {
                if (!short.TryParse(_configuration["AppSettings:ResetPasswordAttempts"], out var resetPasswordMaxAttempts))
                    throw new InvalidOperationException("Não foi possível obter as configurações de segurança para realizar a operação");

                if (_configuration["AppSettings:SecretToken"] is not string secrityToken)
                    throw new InvalidOperationException("Não foi possível obter as configurações de segurança para realizar a operação");

                var (user, resetPassword) = await UserRepository.GetAuthenticated(_connection, model.Username, model.Password, resetPasswordMaxAttempts).AsNoContext();
                
                if (user == null)
                    return BadRequest(new { message = "Usuário ou senha inválidos" });
                
                var authenticateData = TokenService.GerarTokenJwt(secrityToken, user);
                
                user.Password = string.Empty;
                
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
                var users = await UserRepository.GetList(_connection).AsNoContext();

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
                var id = await UserRepository.Insert(_connection, user).AsNoContext();

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
                var sucess = await UserRepository.Delete(_connection, id).AsNoContext();

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
                await UserRepository.Update(_connection, user).AsNoContext();

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

                if (await UserRepository.ResetPassword(_connection, id, resetCode).AsNoContext())
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

                await UserRepository.ChangePassword(_connection, changePassword.Id, changePassword.NewPassword, changePassword.NewPasswordConfirmation, changePassword.CurrentPassword).AsNoContext();

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

        ~AccountController() => Dispose();
    }
}
