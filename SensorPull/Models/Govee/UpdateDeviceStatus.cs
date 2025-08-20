using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class UpdateDeviceStatus
{
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("payload")]
    public UpdateDevicePayload? Payload { get; set; }
}
