using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class Response
{
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public List<Datum>? Data { get; set; }
}
