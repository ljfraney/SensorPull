using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class Datum
{
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("capabilities")]
    public List<Capability>? Capabilities { get; set; }
}
