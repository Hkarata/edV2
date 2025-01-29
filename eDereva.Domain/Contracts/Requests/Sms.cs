using Newtonsoft.Json;

namespace eDereva.Domain.Contracts.Requests;

public record Sms
{
    [JsonProperty(nameof(source_addr))] public string source_addr { get; init; } = "RSAllies";

    [JsonProperty(nameof(schedule_time))] public string schedule_time { get; init; } = string.Empty;

    [JsonProperty(nameof(encoding))] public string encoding { get; init; } = string.Empty;

    [JsonProperty(nameof(message))] public string message { get; set; } = string.Empty;

    [JsonProperty(nameof(recipients))] public List<Recipient>? recipients { get; set; }
}