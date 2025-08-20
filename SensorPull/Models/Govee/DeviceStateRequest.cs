using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class DeviceStateRequest
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }
}

internal class DeviceStateRequestPayload
{
    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }
}