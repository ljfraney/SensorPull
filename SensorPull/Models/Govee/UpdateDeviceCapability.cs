using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class UpdateDeviceCapability
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonPropertyName("value")]
    public int? Value { get; set; }
}
