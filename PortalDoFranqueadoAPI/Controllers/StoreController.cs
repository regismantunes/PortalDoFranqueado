﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalDoFranqueadoAPI.Repositories;
using System.Data.SqlClient;

namespace PortalDoFranqueadoAPI.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController : Controller
    {
        private readonly SqlConnection _connection;

        public StoreController(SqlConnection connection)
            => _connection = (SqlConnection)(connection as ICloneable).Clone();

        ~StoreController()
            => _connection.Dispose();

        [HttpGet]
        [Route("all")]
        [Authorize]
        public async Task<ActionResult<dynamic>> GetStores()
        {
            try
            {
                var stores = await StoreRepository.GetList(_connection);

                return Ok(stores);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public async Task<ActionResult<dynamic>> Get(int id)
        {
            try
            {
                var store = await StoreRepository.Get(_connection, id);

                return Ok(store);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
