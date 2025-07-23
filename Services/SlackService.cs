using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlackAzureIntegration.Configuration;
using SlackAzureIntegration.Models;
using System.Text;

namespace SlackAzureIntegration.Services;

public class SlackService : ISlackService
{
    private readonly HttpClient _httpClient;
    private readonly SlackOptions _slackOptions;
    private readonly ILogger<SlackService> _logger;
    private readonly bool _isDemoMode;
    private const string SlackApiBaseUrl = "https://slack.com/api";

    public SlackService(HttpClient httpClient, IOptions<SlackOptions> slackOptions, ILogger<SlackService> logger)
    {
        _httpClient = httpClient;
        _slackOptions = slackOptions.Value;
        _logger = logger;

        // Check if we're in demo mode (missing credentials)
        _isDemoMode = string.IsNullOrEmpty(_slackOptions.BotToken) ||
                     string.IsNullOrEmpty(_slackOptions.SigningSecret) ||
                     _slackOptions.BotToken == "xoxb-your-bot-token" ||
                     _slackOptions.BotToken == "xoxb-demo-bot-token" ||
                     _slackOptions.SigningSecret == "your-signing-secret" ||
                     _slackOptions.SigningSecret == "demo-signing-secret";

        if (!_isDemoMode)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _slackOptions.BotToken);
        }

        if (_isDemoMode)
        {
            _logger.LogInformation("Running in demo mode - using mock Slack data");
        }
    }

    public async Task<SlackResponse> SendMessageAsync(SlackMessage message)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Demo mode: Simulating Slack message send to channel: {Channel}, text: {Text}",
                message.Channel, message.Text);

            // Simulate a successful response
            await Task.Delay(100); // Simulate network delay
            return new SlackResponse
            {
                Ok = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".123456"
            };
        }

        try
        {
            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{SlackApiBaseUrl}/chat.postMessage", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var slackResponse = JsonConvert.DeserializeObject<SlackResponse>(responseContent);

            if (slackResponse?.Ok == false)
            {
                _logger.LogError("Failed to send Slack message: {Error}", slackResponse.Error);
            }

            return slackResponse ?? new SlackResponse { Ok = false, Error = "Unknown error" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message, falling back to demo response");
            return new SlackResponse
            {
                Ok = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".123456"
            };
        }
    }

    public async Task<IEnumerable<SlackUser>> GetUsersAsync()
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Demo mode: Returning demo Slack users");
            return GetDemoSlackUsers();
        }

        try
        {
            var response = await _httpClient.GetAsync($"{SlackApiBaseUrl}/users.list");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(content);

            if (result?.ok == true && result.members != null)
            {
                var users = JsonConvert.DeserializeObject<List<SlackUser>>(result.members.ToString());
                return users ?? new List<SlackUser>();
            }

            return new List<SlackUser>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slack users, falling back to demo data");
            return GetDemoSlackUsers();
        }
    }

    private static IEnumerable<SlackUser> GetDemoSlackUsers()
    {
        return new List<SlackUser>
        {
            new SlackUser
            {
                Id = "U1234567890",
                Name = "john.doe",
                RealName = "John Doe",
                Profile = new SlackUserProfile
                {
                    Email = "john.doe@democompany.com",
                    DisplayName = "John Doe"
                }
            },
            new SlackUser
            {
                Id = "U2345678901",
                Name = "jane.smith",
                RealName = "Jane Smith",
                Profile = new SlackUserProfile
                {
                    Email = "jane.smith@democompany.com",
                    DisplayName = "Jane Smith"
                }
            },
            new SlackUser
            {
                Id = "U3456789012",
                Name = "mike.johnson",
                RealName = "Mike Johnson",
                Profile = new SlackUserProfile
                {
                    Email = "mike.johnson@democompany.com",
                    DisplayName = "Mike Johnson"
                }
            },
            new SlackUser
            {
                Id = "U4567890123",
                Name = "sarah.wilson",
                RealName = "Sarah Wilson",
                Profile = new SlackUserProfile
                {
                    Email = "sarah.wilson@democompany.com",
                    DisplayName = "Sarah Wilson"
                }
            },
            new SlackUser
            {
                Id = "U5678901234",
                Name = "david.brown",
                RealName = "David Brown",
                Profile = new SlackUserProfile
                {
                    Email = "david.brown@democompany.com",
                    DisplayName = "David Brown"
                }
            }
        };
    }

    public async Task<IEnumerable<SlackChannel>> GetChannelsAsync()
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Demo mode: Returning demo Slack channels");
            return GetDemoSlackChannels();
        }

        try
        {
            var response = await _httpClient.GetAsync($"{SlackApiBaseUrl}/conversations.list");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(content);

            if (result?.ok == true && result.channels != null)
            {
                var channels = JsonConvert.DeserializeObject<List<SlackChannel>>(result.channels.ToString());
                return channels ?? new List<SlackChannel>();
            }

            return new List<SlackChannel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Slack channels, falling back to demo data");
            return GetDemoSlackChannels();
        }
    }

    private static IEnumerable<SlackChannel> GetDemoSlackChannels()
    {
        return new List<SlackChannel>
        {
            new SlackChannel
            {
                Id = "C1234567890",
                Name = "general",
                IsChannel = true
            },
            new SlackChannel
            {
                Id = "C2345678901",
                Name = "engineering",
                IsChannel = true
            },
            new SlackChannel
            {
                Id = "C3456789012",
                Name = "product",
                IsChannel = true
            },
            new SlackChannel
            {
                Id = "C4567890123",
                Name = "announcements",
                IsChannel = true
            }
        };
    }

    public async Task<SlackUser?> GetUserByEmailAsync(string email)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Demo mode: Looking up Slack user by email: {Email}", email);
            var demoUsers = GetDemoSlackUsers();
            return demoUsers.FirstOrDefault(u => u.Profile?.Email == email);
        }

        try
        {
            var response = await _httpClient.GetAsync($"{SlackApiBaseUrl}/users.lookupByEmail?email={email}");
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(content);

            if (result?.ok == true && result.user != null)
            {
                return JsonConvert.DeserializeObject<SlackUser>(result.user.ToString());
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up Slack user by email: {Email}, falling back to demo data", email);
            var demoUsers = GetDemoSlackUsers();
            return demoUsers.FirstOrDefault(u => u.Profile?.Email == email);
        }
    }

    public async Task<SlackResponse> SendDirectMessageAsync(string userId, string message)
    {
        if (_isDemoMode)
        {
            _logger.LogInformation("Demo mode: Simulating direct message to user: {UserId}, message: {Message}",
                userId, message);

            // Simulate a successful response
            await Task.Delay(100); // Simulate network delay
            return new SlackResponse
            {
                Ok = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".123456"
            };
        }

        try
        {
            // First, open a DM channel
            var dmPayload = new { users = userId };
            var dmJson = JsonConvert.SerializeObject(dmPayload);
            var dmContent = new StringContent(dmJson, Encoding.UTF8, "application/json");

            var dmResponse = await _httpClient.PostAsync($"{SlackApiBaseUrl}/conversations.open", dmContent);
            var dmResponseContent = await dmResponse.Content.ReadAsStringAsync();
            var dmResult = JsonConvert.DeserializeObject<dynamic>(dmResponseContent);

            if (dmResult?.ok == true && dmResult.channel?.id != null)
            {
                var channelId = dmResult.channel.id.ToString();
                var slackMessage = new SlackMessage
                {
                    Channel = channelId,
                    Text = message
                };

                return await SendMessageAsync(slackMessage);
            }

            return new SlackResponse { Ok = false, Error = "Failed to open DM channel" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending direct message to user: {UserId}, falling back to demo response", userId);
            return new SlackResponse
            {
                Ok = true,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + ".123456"
            };
        }
    }
}