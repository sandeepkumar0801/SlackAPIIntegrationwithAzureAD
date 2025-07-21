using SlackAzureIntegration.Models;

namespace SlackAzureIntegration.Services;

public interface IAzureAdService
{
    Task<IEnumerable<AzureAdUser>> GetUsersAsync();
    Task<AzureAdUser?> GetUserAsync(string userId);
    Task<IEnumerable<AzureAdGroup>> GetGroupsAsync();
    Task<IEnumerable<AzureAdUser>> GetGroupMembersAsync(string groupId);
    Task<bool> IsUserInGroupAsync(string userId, string groupId);
}