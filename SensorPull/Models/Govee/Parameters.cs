using System.Text.Json.Serialization;

namespace SensorPull.Models.Govee;

public class Parameters
{
    [JsonPropertyName("dataType")]
    public string? DataType { get; set; }

    [JsonPropertyName("options")]
    public List<Option>? Options { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("range")]
    public Range? Range { get; set; }

    [JsonPropertyName("fields")]
    public List<Field>? Fields { get; set; }
}
