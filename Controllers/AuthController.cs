using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlackAzureIntegration.Services;

namespace SlackAzureIntegration.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAzureAdService _azureAdService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAzureAdService azureAdService, ILogger<AuthController> logger)
    {
        _azureAdService = azureAdService;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _azureAdService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        try
        {
            var user = await _azureAdService.GetUserAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        try
        {
            var groups = await _azureAdService.GetGroupsAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("groups/{groupId}/members")]
    public async Task<IActionResult> GetGroupMembers(string groupId)
    {
        try
        {
            var members = await _azureAdService.GetGroupMembersAsync(groupId);
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group members: {GroupId}", groupId);
            return StatusCode(500, "Internal server error");
        }
    }
}
