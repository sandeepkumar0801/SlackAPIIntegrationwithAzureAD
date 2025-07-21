using Microsoft.Graph;
using Microsoft.Graph.Authentication;
using Microsoft.Extensions.Options;
using SlackAzureIntegration.Configuration;
using SlackAzureIntegration.Models;

namespace SlackAzureIntegration.Services;

public class AzureAdService : IAzureAdService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger<AzureAdService> _logger;

    public AzureAdService(IOptions<AzureAdOptions> azureAdOptions, ILogger<AzureAdService> logger)
    {
        _logger = logger;
        
        var options = azureAdOptions.Value;
        var clientSecretCredential = new ClientSecretCredential(
            options.TenantId,
            options.ClientId,
            options.ClientSecret);

        _graphServiceClient = new GraphServiceClient(clientSecretCredential);
    }

    public async Task<IEnumerable<AzureAdUser>> GetUsersAsync()
    {
        try
        {
            var users = await _graphServiceClient.Users.GetAsync();
            
            return users?.Value?.Select(u => new AzureAdUser
            {
                Id = u.Id ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                Email = u.Mail ?? u.UserPrincipalName ?? string.Empty,
                JobTitle = u.JobTitle ?? string.Empty,
                Department = u.Department ?? string.Empty
            }) ?? new List<AzureAdUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure AD users");
            return new List<AzureAdUser>();
        }
    }

    public async Task<AzureAdUser?> GetUserAsync(string userId)
    {
        try
        {
            var user = await _graphServiceClient.Users[userId].GetAsync();
            
            if (user != null)
            {
                return new AzureAdUser
                {
                    Id = user.Id ?? string.Empty,
                    DisplayName = user.DisplayName ?? string.Empty,
                    Email = user.Mail ?? user.UserPrincipalName ?? string.Empty,
                    JobTitle = user.JobTitle ?? string.Empty,
                    Department = user.Department ?? string.Empty
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure AD user: {UserId}", userId);
            return null;
        }
    }

    public async Task<IEnumerable<AzureAdGroup>> GetGroupsAsync()
    {
        try
        {
            var groups = await _graphServiceClient.Groups.GetAsync();
            
            return groups?.Value?.Select(g => new AzureAdGroup
            {
                Id = g.Id ?? string.Empty,
                DisplayName = g.DisplayName ?? string.Empty,
                Description = g.Description ?? string.Empty
            }) ?? new List<AzureAdGroup>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure AD groups");
            return new List<AzureAdGroup>();
        }
    }

    public async Task<IEnumerable<AzureAdUser>> GetGroupMembersAsync(string groupId)
    {
        try
        {
            var members = await _graphServiceClient.Groups[groupId].Members.GetAsync();
            
            return members?.Value?.OfType<User>().Select(u => new AzureAdUser
            {
                Id = u.Id ?? string.Empty,
                DisplayName = u.DisplayName ?? string.Empty,
                Email = u.Mail ?? u.UserPrincipalName ?? string.Empty,
                JobTitle = u.JobTitle ?? string.Empty,
                Department = u.Department ?? string.Empty
            }) ?? new List<AzureAdUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure AD group members: {GroupId}", groupId);
            return new List<AzureAdUser>();
        }
    }

    public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
    {
        try
        {
            var memberOf = await _graphServiceClient.Users[userId].MemberOf.GetAsync();
            return memberOf?.Value?.Any(g => g.Id == groupId) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user is in group: {UserId}, {GroupId}", userId, groupId);
            return false;
        }
    }
}