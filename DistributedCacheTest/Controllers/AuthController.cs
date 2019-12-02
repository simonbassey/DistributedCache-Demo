using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
using DistributedCacheTest.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;

namespace DistributedCacheTest.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ICacheService _cache;

        public AuthController(ICacheService cacheService)
        {
            _cache = cacheService;
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignInUser([FromBody] SignInRequest payload)
        {
            try
            {
                if (payload == null)
                    return BadRequest("Empty request payload");
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!(payload.UserName == "simonbassey" && payload.Password == "123456"))
                    return BadRequest("Invalid login credentials");
                string _authTokenKey = $"{payload.UserName}_authToken";
                if (await _cache.KeyExist(_authTokenKey))
                {
                    await _cache.RemoveEntry(_authTokenKey); // This is were the magic happens to invalidate existing logins on new login
                    // also destroy your existing token from auth system
                }
                var token = Guid.NewGuid().ToString("N"); // call authService to do authentication and return token
                var entryCreated = await _cache.AddEntry(_authTokenKey, token, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(5)); // cache generated token and retrun same
                return Ok(new
                {
                    status = entryCreated ? "success" : "failed",
                    authKey = token
                });

            }
            catch (Exception exception)
            {
                var ex = exception;
                return StatusCode((int)HttpStatusCode.InternalServerError, exception);
            }
        }

    }


}

public class SignInRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

}
