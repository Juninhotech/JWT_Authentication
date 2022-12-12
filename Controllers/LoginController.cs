using JwtAuthentication.Data;
using JwtAuthentication.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _jWTDb;
        private readonly JWTDbContext _jWTDbContext;

        public LoginController(IConfiguration jWTDbt, JWTDbContext jWTDbContext)
        {
            _jWTDb = jWTDbt;
            _jWTDbContext = jWTDbContext;
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            //Authenticate the user
            var  user = Authenticate(userLogin);

            //check if the user is not
            if (user != null)
            {
                //if the user is not null, token will be generated
                var token = Generate(user);
                return Ok(token);
            }
            return NotFound("User not found");
        }

        //generating token method
        private string Generate(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTDb["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.GivenName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Surname, user.Surname)
            };

            //Defining the token object

            var token = new JwtSecurityToken(_jWTDb["Jwt:Issuer"],
                _jWTDb["JWT:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        //Authenticate user method
        private User Authenticate(UserLogin userLogin)
        {
            var currentUser = _jWTDbContext.Users.FirstOrDefault(x => x.Username == userLogin.Username && x.Password == userLogin.Password);

            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }
    }
}
