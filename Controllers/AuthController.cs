using api_InvoicePortal.Dal;
using api_InvoicePortal.Models;
using api_InvoicePortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_InvoicePortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LedgerController> _logger;
        private readonly TokenService _tokenService;

        public AuthController(IConfiguration config, ILogger<LedgerController> logger, TokenService tokenService)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid request");
            }
            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(login.Password);

            DTO dto = new(_config);
            string msg = string.Empty;
            if (dto.Dto_Login(login, out msg))
            //if (login.Username == "admin" && login.Password == "akash@123")
            {
                var token = _tokenService.GenerateToken(login.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid credentials : " + msg);
        }
    }
}
