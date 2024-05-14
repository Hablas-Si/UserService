using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Models;
using Repositories;
using System.Text;
using System.Security.Authentication.ExtendedProtection;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMongoDBRepository _mongoDBRepository;

        public UserController(ILogger<UserController> logger, IMongoDBRepository mongoDBRepository)
        {
            _logger = logger;
            _mongoDBRepository = mongoDBRepository;

        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel login)
        {
            //Tjekker om brugeren allerede eksisterer
            if (await _mongoDBRepository.CheckIfUserExistsAsync(login.Username) == true)
            {
                return BadRequest("User already exists");
            }

            await _mongoDBRepository.AddUserAsync(login);
            return Ok("User created");
        }

        [HttpGet("getuser/{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _mongoDBRepository.FindUserAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("updateuser/{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserModel login)
        {
            // henter først en user vha. FindUser metode udfra id bestemt i UpdateUser parametren.
            var existingUser = await _mongoDBRepository.FindUserAsync(id);
            //Gør måske ikke rigtigt noget, da den allerede sender 400 bad request. fikser senere.
            if (existingUser == null) 
            {
                return NotFound();
            }

            // Opdaterer de relevante felter på den eksisterende bruger
            existingUser.Username = login.Username;
            existingUser.Password = login.Password;
            existingUser.Role = login.Role;
            existingUser.Email = login.Email;

            // Kalder metode til at opdatere brugeren i databasen
            await _mongoDBRepository.UpdateUserAsync(existingUser);

            // returner statuskode 204
            return NoContent();
        }

        [HttpDelete("deleteuser/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // henter først en vare vha. GetVare metode udfra id bestemt i UpdateVare parametren.
            var existingUser = await _mongoDBRepository.FindUserAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // ovenover matchede vi id med id fra paramtre og nu sletter vi den
            await _mongoDBRepository.DeleteUserAsync(id);

            // statuskode 204
            return NoContent();
        }

    }
}
