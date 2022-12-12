using JwtAuthentication.Data;
using JwtAuthentication.Model;
using JwtAuthentication.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JWTDbContext _jWTDbContext;

        public UsersController(JWTDbContext jWTDbContext)
        {
            _jWTDbContext = jWTDbContext;
        }



        [HttpGet]
        public async Task<IActionResult>GetUser()
        {
            var users = await _jWTDbContext.Users.Select(x => new User
            {
                Id = x.Id,
                Username = x.Username,
                Password = x.Password,
                Email = x.Email,
                Role = x.Role,
                GivenName = x.GivenName,
                Surname = x.Surname,
            }).ToListAsync();

            return Ok(users);
        }

        

        [HttpPost]
        public async Task<IActionResult>PostUser([FromBody] UserrViewModel userrViewModel)
        {
            var user = new User
            {
                Username = userrViewModel.Username,
                Password = userrViewModel.Password,
                GivenName = userrViewModel.GivenName,
                Surname = userrViewModel.Surname,
                Email = userrViewModel.Email,
                Role = userrViewModel.Role
            };
            await _jWTDbContext.Users.AddAsync(user);
            await _jWTDbContext.SaveChangesAsync();

            return Ok("User added successfully");
        }

        [HttpGet("Developers")]
        [Authorize (Roles = "Developer")]
        public IActionResult Developer()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Welcome {currentUser.Username}, you are a {currentUser.Role}");
        }

        private User GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if(identity !=null)
            {
                var userClaims = identity.Claims;

                return new User
                {
                    Username = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value,
                    Email = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value,
                    GivenName = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName).Value,
                    Surname = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Surname).Value,
                    Role = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value


                };

            }
            return null;
        }
    }
}
