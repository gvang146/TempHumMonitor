using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMTechNodeAPI.Data;
using MMTechNodeAPI.Models;

namespace MMTechNodeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly MMTechDbContext _context;
        
        public UserController(MMTechDbContext context)
        {
            _context = context;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(User user)
        {
            if (string.IsNullOrEmpty(user.Name))
                return BadRequest("User name cannot be emptied");
            if (string.IsNullOrEmpty(user.Password))
                return BadRequest("Password cannot be emptied");
            
            var passHash = ComputeStringToSha256Hash(user.Password);

            var usr = await _context.Users
                .FirstOrDefaultAsync(u => u.Name.Equals(user.Name) && u.Password.Equals(passHash));

            if (usr == null) return NotFound("Invalid user name or password.");

            return new { Id = usr.Id, Name = usr.Name };
        }

        [HttpPost("create")]
        public async Task<ActionResult<object>> Create(User user)
        {
            if (string.IsNullOrEmpty(user.Name))
                return BadRequest("User name cannot be emptied");
            if (string.IsNullOrEmpty(user.Password))
                return BadRequest("Password cannot be emptied");
            
            var usr = await _context.Users
                .FirstOrDefaultAsync(u => u.Name.Equals(user.Name));

            if (usr != null) return Conflict("User name already exist.");
            
            user.Id = Guid.NewGuid();
            user.Password = ComputeStringToSha256Hash(user.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new { Id = user.Id, Name = user.Name };
        }

        private static string ComputeStringToSha256Hash(string plainText)
        {
            // Create a SHA256 hash from string   
            using var sha256Hash = SHA256.Create();
            // Computing Hash - returns here byte array
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            // now convert byte array to a string   
            var strBuilder = new StringBuilder();
            foreach (var b in bytes)
            {
                strBuilder.Append(b.ToString("x2"));
            }
            return strBuilder.ToString();
        }
    }
}