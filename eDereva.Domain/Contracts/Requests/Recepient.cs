using Newtonsoft.Json;

namespace eDereva.Domain.Contracts.Requests;

public record Recipient
{
    [JsonProperty(nameof(recipient_id))] public string recipient_id { get; set; } = string.Empty;

    [JsonProperty(nameof(dest_addr))] public string dest_addr { get; set; } = string.Empty;
}