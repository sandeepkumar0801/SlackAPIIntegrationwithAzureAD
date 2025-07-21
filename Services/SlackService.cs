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
    private const string SlackApiBaseUrl = "https://slack.com/api";

    public SlackService(HttpClient httpClient, IOptions<SlackOptions> slackOptions, ILogger<SlackService> logger)
    {
        _httpClient = httpClient;
        _slackOptions = slackOptions.Value;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _slackOptions.BotToken);
    }

    public async Task<SlackResponse> SendMessageAsync(SlackMessage message)
    {
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
            _logger.LogError(ex, "Error sending Slack message");
            return new SlackResponse { Ok = false, Error = ex.Message };
        }
    }

    public async Task<IEnumerable<SlackUser>> GetUsersAsync()
    {
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
            _logger.LogError(ex, "Error getting Slack users");
            return new List<SlackUser>();
        }
    }

    public async Task<IEnumerable<SlackChannel>> GetChannelsAsync()
    {
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
            _logger.LogError(ex, "Error getting Slack channels");
            return new List<SlackChannel>();
        }
    }

    public async Task<SlackUser?> GetUserByEmailAsync(string email)
    {
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
            _logger.LogError(ex, "Error looking up Slack user by email: {Email}", email);
            return null;
        }
    }

    public async Task<SlackResponse> SendDirectMessageAsync(string userId, string message)
    {
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
            _logger.LogError(ex, "Error sending direct message to user: {UserId}", userId);
            return new SlackResponse { Ok = false, Error = ex.Message };
        }
    }
}