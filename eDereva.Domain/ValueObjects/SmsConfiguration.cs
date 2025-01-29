namespace eDereva.Domain.ValueObjects;

public class SmsConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string SourceAddress { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
}