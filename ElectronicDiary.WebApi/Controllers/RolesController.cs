using ElectronicDiary.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElectronicDiary.WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested the list of all roles", userId);

            var roles = await _roleManager.Roles.ToListAsync();
            if (roles.Count == 0)
            {
                _logger.LogWarning("The role list is empty");
                return NotFound("No roles found.");
            }

            return Ok(roles.Select(r => r.Name));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddRole([FromBody] string roleName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to add role: {Role}", userId, roleName);

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("User with ID {UserId} attempted to add an existing role: {Role}", userId, roleName);
                return BadRequest("Role already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} successfully added role: {Role}", userId, roleName);
                return Ok($"Role {roleName} created successfully.");
            }

            _logger.LogError("Error adding role {Role} by user with ID {UserId}: {Errors}", roleName, userId, result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpDelete("delete/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to delete role: {Role}", userId, roleName);

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to delete a non-existent role: {Role}", userId, roleName);
                return NotFound($"Role {roleName} not found.");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} successfully deleted role: {Role}", userId, roleName);
                return Ok($"Role {roleName} deleted successfully.");
            }

            _logger.LogError("Error deleting role {Role} by user with ID {UserId}: {Errors}", roleName, userId, result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser([FromQuery] string username, [FromQuery] string roleName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to assign role {Role} to user {Username}", userId, roleName, username);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to assign role to a non-existent user {Username}", userId, username);
                return NotFound($"User {username} not found.");
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("User with ID {UserId} attempted to assign a non-existent role {Role}", userId, roleName);
                return NotFound($"Role {roleName} not found.");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} successfully assigned role {Role} to user {Username}", userId, roleName, username);
                return Ok($"Role {roleName} assigned to user {username} successfully.");
            }

            _logger.LogError("Error assigning role {Role} to user {Username} by admin {UserId}: {Errors}", roleName, username, userId, result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRoleFromUser([FromQuery] string username, [FromQuery] string roleName)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to remove role {Role} from user {Username}", userId, roleName, username);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to remove role from a non-existent user {Username}", userId, username);
                return NotFound($"User {username} not found.");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} successfully removed role {Role} from user {Username}", userId, roleName, username);
                return Ok($"Role {roleName} removed from user {username} successfully.");
            }

            _logger.LogError("Error removing role {Role} from user {Username} by user with ID {UserId}: {Errors}", roleName, username, userId, result.Errors);
            return BadRequest(result.Errors);
        }
    }

}