using Microsoft.AspNetCore.Mvc;
using SlackAzureIntegration.Services;

namespace SlackAzureIntegration.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly IAzureAdService _azureAdService;
    private readonly ISlackService _slackService;
    private readonly ILogger<DemoController> _logger;

    public DemoController(
        IAzureAdService azureAdService,
        ISlackService slackService,
        ILogger<DemoController> logger)
    {
        _azureAdService = azureAdService;
        _slackService = slackService;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "Running",
            Mode = "Demo",
            Timestamp = DateTime.UtcNow,
            Message = "Slack-Azure AD Integration API is running in demo mode"
        });
    }

    [HttpGet("azure-users")]
    public async Task<IActionResult> GetAzureUsers()
    {
        try
        {
            var users = await _azureAdService.GetUsersAsync();
            return Ok(new
            {
                Success = true,
                Count = users.Count(),
                Users = users,
                Message = "Demo Azure AD users retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure users");
            return StatusCode(500, new { Error = "Failed to retrieve Azure users" });
        }
    }

    [HttpGet("slack-users")]
    public async Task<IActionResult> GetSlackUsers()
    {
        try
        {
            var users = await _slackService.GetUsersAsync();
            return Ok(new
            {
                Success = true,
                Count = users.Count(),
                Users = users,
                Message = "Demo Slack users retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slack users");
            return StatusCode(500, new { Error = "Failed to retrieve Slack users" });
        }
    }

    [HttpGet("integration-test")]
    public async Task<IActionResult> TestIntegration()
    {
        try
        {
            var azureUsers = await _azureAdService.GetUsersAsync();
            var slackUsers = await _slackService.GetUsersAsync();
            var azureGroups = await _azureAdService.GetGroupsAsync();
            var slackChannels = await _slackService.GetChannelsAsync();

            return Ok(new
            {
                Success = true,
                Integration = new
                {
                    AzureAD = new
                    {
                        UsersCount = azureUsers.Count(),
                        GroupsCount = azureGroups.Count(),
                        SampleUser = azureUsers.FirstOrDefault()
                    },
                    Slack = new
                    {
                        UsersCount = slackUsers.Count(),
                        ChannelsCount = slackChannels.Count(),
                        SampleUser = slackUsers.FirstOrDefault()
                    }
                },
                Message = "Integration test completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during integration test");
            return StatusCode(500, new { Error = "Integration test failed" });
        }
    }
}
