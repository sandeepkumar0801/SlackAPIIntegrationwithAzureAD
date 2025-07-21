using Newtonsoft.Json;

namespace SlackAzureIntegration.Models;

public class SlackMessage
{
    [JsonProperty("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonProperty("text")]
    public string Text { get; set; } = string.Empty;

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("icon_emoji")]
    public string? IconEmoji { get; set; }
}

public class SlackResponse
{
    [JsonProperty("ok")]
    public bool Ok { get; set; }

    [JsonProperty("error")]
    public string? Error { get; set; }

    [JsonProperty("ts")]
    public string? Timestamp { get; set; }
}

public class SlackUser
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("real_name")]
    public string RealName { get; set; } = string.Empty;

    [JsonProperty("profile")]
    public SlackUserProfile? Profile { get; set; }
}

public class SlackUserProfile
{
    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("display_name")]
    public string? DisplayName { get; set; }
}

public class SlackChannel
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("is_channel")]
    public bool IsChannel { get; set; }
}