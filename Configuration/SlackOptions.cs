namespace SlackAzureIntegration.Configuration;

public class SlackOptions
{
    public string BotToken { get; set; } = string.Empty;
    public string AppToken { get; set; } = string.Empty;
    public string SigningSecret { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}