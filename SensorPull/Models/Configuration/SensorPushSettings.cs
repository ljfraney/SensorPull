namespace SensorPull.Models.Configuration;

public class SensorPushSettings
{
    public string? BaseUrl { get; set; }

    public string? AuthEndpoint { get; set; }

    public string? AccessTokenEndpoint { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? SensorIdOrName { get; set; }    
}
