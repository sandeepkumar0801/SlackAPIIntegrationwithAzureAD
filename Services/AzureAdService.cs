using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Extensions.Options;
using SlackAzureIntegration.Configuration;
using SlackAzureIntegration.Models;
using Azure.Identity;

namespace SlackAzureIntegration.Services;

public class AzureAdService : IAzureAdService
{
    private readonly GraphServiceClient? _graphServiceClient;
    private readonly ILogger<AzureAdService> _logger;
    private readonly bool _isDemoMode;

    public AzureAdService(IOptions<AzureAdOptions> azureAdOptions, ILogger<AzureAdService> logger)
    {
        _logger = logger;

        var options = azureAdOptions.Value;

        // Check if we're in demo mode (missing credentials)
        _isDemoMode = string.IsNullOrEmpty(options.TenantId) ||
                     string.IsNullOrEmpty(options.ClientId) ||
                     string.IsNullOrEmpty(options.ClientSecret) ||
                     options.TenantId == "your-tenant-id" ||
                     options.ClientId == "your-client-id" ||
                     options.ClientSecret == "your-client-secret";

        if (!_isDemoMode)
        {
            try
            {
                var clientSecretCredential = new ClientSecretCredential(
                    options.TenantId,
                    options.ClientId,
                    options.ClientSecret);

                _graphServiceClient = new GraphServiceClient(clientSecretCredential);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Graph client, falling back to demo mode");
                _isDemoMode = true;
            }
        }

        if (_isDemoMode)
        {
            _logger.LogInformation("Running in demo mode - using mock Azure AD data");
        }
    }

    public async Task<IEnumerable<AzureAdUser>> GetUsersAsync()
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Returning demo Azure AD users");
            return GetDemoUsers();
        }

        try
        {
            var users = await _graphServiceClient!.Users.GetAsync();

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
            _logger.LogError(ex, "Error getting Azure AD users, falling back to demo data");
            return GetDemoUsers();
        }
    }

    private static IEnumerable<AzureAdUser> GetDemoUsers()
    {
        return new List<AzureAdUser>
        {
            new AzureAdUser
            {
                Id = "demo-user-1",
                DisplayName = "John Doe",
                Email = "john.doe@democompany.com",
                JobTitle = "Software Engineer",
                Department = "Engineering"
            },
            new AzureAdUser
            {
                Id = "demo-user-2",
                DisplayName = "Jane Smith",
                Email = "jane.smith@democompany.com",
                JobTitle = "Product Manager",
                Department = "Product"
            },
            new AzureAdUser
            {
                Id = "demo-user-3",
                DisplayName = "Mike Johnson",
                Email = "mike.johnson@democompany.com",
                JobTitle = "DevOps Engineer",
                Department = "Engineering"
            },
            new AzureAdUser
            {
                Id = "demo-user-4",
                DisplayName = "Sarah Wilson",
                Email = "sarah.wilson@democompany.com",
                JobTitle = "UX Designer",
                Department = "Design"
            },
            new AzureAdUser
            {
                Id = "demo-user-5",
                DisplayName = "David Brown",
                Email = "david.brown@democompany.com",
                JobTitle = "Team Lead",
                Department = "Engineering"
            }
        };
    }

    public async Task<AzureAdUser?> GetUserAsync(string userId)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Returning demo Azure AD user for ID: {UserId}", userId);
            return GetDemoUsers().FirstOrDefault(u => u.Id == userId);
        }

        try
        {
            var user = await _graphServiceClient!.Users[userId].GetAsync();

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
            _logger.LogError(ex, "Error getting Azure AD user: {UserId}, falling back to demo data", userId);
            return GetDemoUsers().FirstOrDefault(u => u.Id == userId);
        }
    }

    public async Task<IEnumerable<AzureAdGroup>> GetGroupsAsync()
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Returning demo Azure AD groups");
            return GetDemoGroups();
        }

        try
        {
            var groups = await _graphServiceClient!.Groups.GetAsync();

            return groups?.Value?.Select(g => new AzureAdGroup
            {
                Id = g.Id ?? string.Empty,
                DisplayName = g.DisplayName ?? string.Empty,
                Description = g.Description ?? string.Empty
            }) ?? new List<AzureAdGroup>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Azure AD groups, falling back to demo data");
            return GetDemoGroups();
        }
    }

    private static IEnumerable<AzureAdGroup> GetDemoGroups()
    {
        return new List<AzureAdGroup>
        {
            new AzureAdGroup
            {
                Id = "demo-group-1",
                DisplayName = "Engineering Team",
                Description = "All engineering staff and developers"
            },
            new AzureAdGroup
            {
                Id = "demo-group-2",
                DisplayName = "Product Team",
                Description = "Product managers and designers"
            },
            new AzureAdGroup
            {
                Id = "demo-group-3",
                DisplayName = "Leadership Team",
                Description = "Company leadership and executives"
            }
        };
    }

    public async Task<IEnumerable<AzureAdUser>> GetGroupMembersAsync(string groupId)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Returning demo group members for group: {GroupId}", groupId);
            return GetDemoGroupMembers(groupId);
        }

        try
        {
            var members = await _graphServiceClient!.Groups[groupId].Members.GetAsync();

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
            _logger.LogError(ex, "Error getting Azure AD group members: {GroupId}, falling back to demo data", groupId);
            return GetDemoGroupMembers(groupId);
        }
    }

    public async Task<bool> IsUserInGroupAsync(string userId, string groupId)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Checking demo group membership for user: {UserId}, group: {GroupId}", userId, groupId);
            var groupMembers = GetDemoGroupMembers(groupId);
            return groupMembers.Any(u => u.Id == userId);
        }

        try
        {
            var memberOf = await _graphServiceClient!.Users[userId].MemberOf.GetAsync();
            return memberOf?.Value?.Any(g => g.Id == groupId) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user is in group: {UserId}, {GroupId}", userId, groupId);
            return false;
        }
    }

    private static IEnumerable<AzureAdUser> GetDemoGroupMembers(string groupId)
    {
        var allUsers = GetDemoUsers().ToList();

        return groupId switch
        {
            "demo-group-1" => allUsers.Where(u => u.Department == "Engineering"), // Engineering Team
            "demo-group-2" => allUsers.Where(u => u.Department == "Product" || u.Department == "Design"), // Product Team
            "demo-group-3" => allUsers.Where(u => u.JobTitle.Contains("Lead") || u.JobTitle.Contains("Manager")), // Leadership Team
            _ => new List<AzureAdUser>()
        };
    }
}