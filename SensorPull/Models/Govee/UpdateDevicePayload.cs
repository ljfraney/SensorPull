using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class UpdateDevicePayload
{
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("capability")]
    public UpdateDeviceCapability? Capability { get; set; }
}
