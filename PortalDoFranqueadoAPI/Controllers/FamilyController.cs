﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Repositories;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/family")]
    [ApiController]
    public class FamilyController : Controller
    {
        private readonly SqlConnection _connection;

        public FamilyController(SqlConnection connection)
            => _connection = connection;

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFamilies()
            => await GetFamilies(false);

        [HttpGet]
        [Route("all/withsizes")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetFamiliesWithSizes()
            => await GetFamilies(true);

        private async Task<ActionResult<dynamic>> GetFamilies(bool withSizes)
        {
            try
            {
                var families = await FamilyRepository.GetFamilies(_connection, withSizes);

                return Ok(families);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
