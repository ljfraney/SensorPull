using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class Range
{
    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }

    [JsonPropertyName("precision")]
    public int? Precision { get; set; }
}
