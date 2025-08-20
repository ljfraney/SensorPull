using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class Capability
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonPropertyName("parameters")]
    public Parameters? Parameters { get; set; }
}
