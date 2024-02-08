using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProductsManager.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProductsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost ("Register")]
        public ActionResult<User> Register(UserDTO request)
        {
            string passwordhash 
                 = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.UserName = request.UserName;
            user.PasswordHash = passwordhash;
            user.Email = request.Email;
            return Ok(user);
        }


        [HttpPost("login")]
        public ActionResult<User> Login(UserDTO request)
        {
            if (user.UserName != request.UserName)
            {
                return BadRequest("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Wrong password.");
            }

            string Token = CreateToken(user);

            return Ok(Token);
        }

        private string CreateToken (User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!)
                );

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                     claims: claims,
                     expires: DateTime.Now.AddHours(1),
                     signingCredentials: cred
                 );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
