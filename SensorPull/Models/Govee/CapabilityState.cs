using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class CapabilityState
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
