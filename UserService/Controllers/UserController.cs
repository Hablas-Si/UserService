using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Models;
using Repositories;
using System.Text;
using System.Security.Authentication.ExtendedProtection;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IMongoDBRepository _mongoDBRepository;

        private readonly IVaultRepository _vaultService;


        public UserController(ILogger<UserController> logger, IMongoDBRepository mongoDBRepository, IVaultRepository vaultService)
        {
            _logger = logger;
            _mongoDBRepository = mongoDBRepository;
            _vaultService = vaultService;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel login)
        {
            //TjekkerR om brugeren allerede eksisterer
            if (await _mongoDBRepository.CheckIfUserExistsAsync(login.Username) == true)
            {
                return BadRequest("User already exists");
            }

            await _mongoDBRepository.AddUserAsync(login);
            return Ok("User created");
        }

        [HttpGet("getuser/{id}"), Authorize(Roles="Admin")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _mongoDBRepository.FindUserAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("updateuser/{id}"), Authorize(Roles="Admin")]
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


        [HttpDelete("deleteuser/{id}"), Authorize(Roles="Admin")]
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
        [AllowAnonymous]
        [HttpPost("login/validate")]
        public async Task<IActionResult> ValidateUser([FromBody] UserModel login)
        {
            // Implementer logik til at validere brugeroplysningerne mod databasen eller anden autentificeringsmekanisme
            var isValidUser = await _mongoDBRepository.CheckIfUserExistsWithPassword(login.Username, login.Password, login.Role);
            return Ok(isValidUser);
        }

        //TEST ENVIRONMENT
        // OBS: TIlføj en Authorize attribute til metoderne nedenunder Kig ovenfor i jwt token creation.
        [HttpGet("authorized"), Authorize(Roles = "Admin")]
        public IActionResult Authorized()
        {

            // Hvis brugeren har en gyldig JWT-token og rollen "Admin", vil denne metode blive udført
            return Ok("You are authorized to access this resource.");
        }

        // En get der henter secrets ned fra vault
        [AllowAnonymous]
        [HttpGet("getsecret/{path}")]
        public async Task<IActionResult> GetSecret(string path)
        {
            try
            {
                _logger.LogInformation($"Getting secret with path {path}");
                var secretValue = await _vaultService.GetSecretAsync(path);
                if (secretValue != null)
                {
                    return Ok(secretValue);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving secret: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving secret.");
            }
        }


    }
}
