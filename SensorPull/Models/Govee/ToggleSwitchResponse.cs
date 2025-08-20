using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class ToggleSwitchResponse
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("msg")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("capability")]
    public CapabilityStatus? Capability { get; set; }
}
