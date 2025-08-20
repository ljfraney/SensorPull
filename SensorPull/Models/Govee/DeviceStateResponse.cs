using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public sealed class DeviceStateResponse
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("msg")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("payload")]
    public DeviceStatePayload? Payload { get; set; }
}

public sealed class DeviceStatePayload
{
    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("device")]
    public string? Device { get; set; }

    [JsonPropertyName("capabilities")]
    public List<DeviceStateCapability> Capabilities { get; set; } = [];
}

public sealed class DeviceStateCapability
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("instance")]
    public string Instance { get; set; } = "";

    [JsonPropertyName("state")]
    public DeviceStateCapabilityState State { get; set; } = new();
}

public sealed class DeviceStateCapabilityState
{
    [JsonPropertyName("value")]
    public object? Value { get; set; }
}
