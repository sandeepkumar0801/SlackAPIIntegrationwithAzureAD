using SlackAzureIntegration.Models;

namespace SlackAzureIntegration.Services;

public interface ISlackService
{
    Task<SlackResponse> SendMessageAsync(SlackMessage message);
    Task<IEnumerable<SlackUser>> GetUsersAsync();
    Task<IEnumerable<SlackChannel>> GetChannelsAsync();
    Task<SlackUser?> GetUserByEmailAsync(string email);
    Task<SlackResponse> SendDirectMessageAsync(string userId, string message);
}