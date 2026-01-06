using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentMonitor.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/usermanagement
        [HttpGet]
        public async Task<ActionResult<List<UserInfoDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        // GET api/usermanagement/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserInfoDto>> GetUser(string userId)
        {
            var user = await _userService.GetUser(userId);
            
            if (user == null)
            {
                return NotFound(new { Message = $"User with id '{userId}' not found" });
            }

            return Ok(user);
        }

        // DELETE api/usermanagement/{userId}
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _userService.DeleteUser(userId);
            return Ok(new { Message = $"User {userId} deleted" });
        }

        // POST api/usermanagement/claims
        [HttpPost("claims")]
        public async Task<IActionResult> ManageUserClaims([FromBody] ManageUserClaimsRequest request)
        {
            await _userService.ManageUserClaims(request);
            
            var addedCount = request.ClaimsToAdd?.Count ?? 0;
            var removedCount = request.ClaimsToRemove?.Count ?? 0;
            
            return Ok(new { 
                Message = $"Claims managed for user {request.UserId}. Added: {addedCount}, Removed: {removedCount}",
                ClaimsAdded = addedCount,
                ClaimsRemoved = removedCount
            });
        }

        // POST api/usermanagement/roles
        [HttpPost("roles")]
        public async Task<IActionResult> ManageUserRoles([FromBody] ManageUserRolesRequest request)
        {
            await _userService.ManageUserRoles(request);
            
            var addedCount = request.RolesToAdd?.Count ?? 0;
            var removedCount = request.RolesToRemove?.Count ?? 0;
            
            return Ok(new { 
                Message = $"Roles managed for user {request.UserId}. Added: {addedCount}, Removed: {removedCount}",
                RolesAdded = addedCount,
                RolesRemoved = removedCount
            });
        }
    }
}
