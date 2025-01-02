using PortalDoFranqueadoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using System.Linq;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(IConfiguration configuration, IUserRepository userRepository) : ControllerBase, IDisposable
    {
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] UserInput model)
        {
            if (!short.TryParse(configuration["AppSettings:ResetPasswordAttempts"], out var resetPasswordMaxAttempts))
                throw new InvalidOperationException("Não foi possível obter as configurações de segurança para realizar a operação");

            if (configuration["AppSettings:SecretToken"] is not string secrityToken)
                throw new InvalidOperationException("Não foi possível obter as configurações de segurança para realizar a operação");

            var (user, resetPassword) = await userRepository.GetAuthenticated(model.Username, model.Password, resetPasswordMaxAttempts).AsNoContext();

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

        [HttpGet]
        [Route("users/all")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> GetUsers()
        {
            var users = await userRepository.GetList().AsNoContext();
            return Ok(users);
        }

        [HttpPost]
        [Route("users")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Insert([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.First().Errors.First().ErrorMessage);

            var id = await userRepository.Insert(user).AsNoContext();
            return Ok(id);
        }

        [HttpDelete]
        [Route("users/{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Delete(int id)
        {
            var sucess = await userRepository.Delete(id).AsNoContext();
            return Ok(sucess);
        }

        [HttpPut]
        [Route("users")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> Update([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.First().Errors.First().ErrorMessage);

            await userRepository.Update(user).AsNoContext();
            return Ok();
        }

        [HttpPut]
        [Route("users/pswreset")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<dynamic>> ResetPassword([FromBody] int id)
        {
            var resetCode = Random.Shared.Next(0, 999999).ToString("000000");

            if (await userRepository.ResetPassword(id, resetCode).AsNoContext())
                return Ok(resetCode);

            return BadRequest(new { message = "O usuário não foi encontrado." });
        }

        [HttpPut]
        [Route("users/pswchange")]
        [Authorize]
        public async Task<ActionResult<dynamic>> ChangePassword([FromBody] UserChangePassword changePassword)
        {
                if (!User.Identity.Name.Equals(changePassword.Id.ToString()))
                    throw new Exception("O código do usuário informado deve ser igual ao código do usuário atual.");

                await userRepository.ChangePassword(changePassword.Id, changePassword.NewPassword, changePassword.NewPasswordConfirmation, changePassword.CurrentPassword).AsNoContext();

                return Ok();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~AccountController() => Dispose();
    }
}
