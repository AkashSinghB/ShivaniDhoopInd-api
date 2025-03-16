using api_InvoicePortal.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace api_InvoicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly TokenService _tokenService;

        public AuthController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid request");
            }

            // Simulate user validation (replace with real DB check)
            if (login.Username == "admin" && login.Password == "password")
            {
                var token = _tokenService.GenerateToken(login.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid credentials");
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
