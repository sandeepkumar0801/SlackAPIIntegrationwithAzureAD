using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlackAzureIntegration.Models;
using SlackAzureIntegration.Services;

namespace SlackAzureIntegration.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SlackController : ControllerBase
{
    private readonly ISlackService _slackService;
    private readonly IAzureAdService _azureAdService;
    private readonly ILogger<SlackController> _logger;

    public SlackController(
        ISlackService slackService, 
        IAzureAdService azureAdService,
        ILogger<SlackController> logger)
    {
        _slackService = slackService;
        _azureAdService = azureAdService;
        _logger = logger;
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SlackMessage message)
    {
        try
        {
            var response = await _slackService.SendMessageAsync(message);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetSlackUsers()
    {
        try
        {
            var users = await _slackService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slack users");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels()
    {
        try
        {
            var channels = await _slackService.GetChannelsAsync();
            return Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slack channels");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("notify-azure-users")]
    public async Task<IActionResult> NotifyAzureUsers([FromBody] NotifyUsersRequest request)
    {
        try
        {
            var azureUsers = await _azureAdService.GetUsersAsync();
            var results = new List<object>();

            foreach (var azureUser in azureUsers)
            {
                if (!string.IsNullOrEmpty(azureUser.Email))
                {
                    var slackUser = await _slackService.GetUserByEmailAsync(azureUser.Email);
                    if (slackUser != null)
                    {
                        var response = await _slackService.SendDirectMessageAsync(slackUser.Id, request.Message);
                        results.Add(new
                        {
                            AzureUser = azureUser.DisplayName,
                            SlackUser = slackUser.Name,
                            Success = response.Ok,
                            Error = response.Error
                        });
                    }
                }
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying Azure users via Slack");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("notify-group")]
    public async Task<IActionResult> NotifyGroup([FromBody] NotifyGroupRequest request)
    {
        try
        {
            var groupMembers = await _azureAdService.GetGroupMembersAsync(request.GroupId);
            var results = new List<object>();

            foreach (var member in groupMembers)
            {
                if (!string.IsNullOrEmpty(member.Email))
                {
                    var slackUser = await _slackService.GetUserByEmailAsync(member.Email);
                    if (slackUser != null)
                    {
                        var response = await _slackService.SendDirectMessageAsync(slackUser.Id, request.Message);
                        results.Add(new
                        {
                            AzureUser = member.DisplayName,
                            SlackUser = slackUser.Name,
                            Success = response.Ok,
                            Error = response.Error
                        });
                    }
                }
            }

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying group via Slack: {GroupId}", request.GroupId);
            return StatusCode(500, "Internal server error");
        }
    }
}

public class NotifyUsersRequest
{
    public string Message { get; set; } = string.Empty;
}

public class NotifyGroupRequest
{
    public string GroupId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}