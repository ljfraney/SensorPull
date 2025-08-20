using SensorPull.Models.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace SensorPull.Services;

public class SensorPushClient(IHttpClientFactory httpFactory, SensorPushSettings settings)
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly SensorPushSettings _settings = settings;
    private async Task<string> GetAccessTokenAsync()
    {
        var http = _httpFactory.CreateClient();

        // Step 1: signin -> authorization code
        var authResp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/{_settings.AuthEndpoint}",
            new { email = settings.Email, password = _settings.Password }
        );
        authResp.EnsureSuccessStatusCode();

        var authJson = await authResp.Content.ReadFromJsonAsync<JsonElement>();
        var authorization = authJson.GetProperty("authorization").GetString();

        // Step 2: exchange authorization -> access token
        var tokenResp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/{_settings.AccessTokenEndpoint}",
            new { authorization }
        );
        tokenResp.EnsureSuccessStatusCode();

        var tokenJson = await tokenResp.Content.ReadFromJsonAsync<JsonElement>();

        // bearer for subsequent calls
        return tokenJson.GetProperty("accesstoken").GetString()!;
    }

    public async Task<(double minF, double maxF)> GetTemperatureAlertThresholdsAsync(string sensorIdOrName)
    {
        var accessToken = await GetAccessTokenAsync();
        var sensorId = await ResolveSensorIdAsync(accessToken, sensorIdOrName);

        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Authorization", accessToken);

        // Per docs: POST with empty body to list sensors
        var resp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/devices/sensors",
            new { });
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();

        // Some accounts wrap sensors in { "sensors": { ... } }, yours is top-level keyed by sensorId
        var sensorsObj = json.TryGetProperty("sensors", out var wrapped) ? wrapped : json;

        if (!sensorsObj.TryGetProperty(sensorId, out var sensorObj))
        {
            throw new InvalidOperationException($"Sensor ID {sensorId} not found in response.");
        }

        var alertsObj = sensorObj.GetProperty("alerts").GetProperty("temperature");
        var min = alertsObj.GetProperty("min").GetDouble();
        var max = alertsObj.GetProperty("max").GetDouble();

        return (min, max);
    }

    private async Task<string> ResolveSensorIdAsync(string accessToken, string sensorIdOrName)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Authorization", accessToken);

        // List sensors
        var sensorsResp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/devices/sensors",
            new { }); // empty body per examples
        sensorsResp.EnsureSuccessStatusCode();
        var sensorsJson = await sensorsResp.Content.ReadFromJsonAsync<JsonElement>();

        // Some accounts return { "sensors": { ... } }, others return { "<id>": { ... } }
        var sensorsObj = sensorsJson.TryGetProperty("sensors", out var wrapped) ? wrapped : sensorsJson;

        // Try ID match first
        foreach (var kvp in sensorsObj.EnumerateObject())
        {
            if (string.Equals(kvp.Name, sensorIdOrName, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Name;
            }
        }

        // Try name match
        foreach (var kvp in sensorsObj.EnumerateObject())
        {
            if (kvp.Value.TryGetProperty("name", out var nameProp))
            {
                var name = nameProp.GetString();
                if (!string.IsNullOrWhiteSpace(name) &&
                    string.Equals(name, sensorIdOrName, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Name; // return the sensorId (e.g., "400311.16254137713499936053")
                }
            }
        }

        throw new InvalidOperationException($"Sensor '{sensorIdOrName}' not found.");
    }

    public async Task<double> GetLatestTemperatureFAsync(string sensorIdOrName)
    {
        var accessToken = await GetAccessTokenAsync();
        var sensorId = await ResolveSensorIdAsync(accessToken, sensorIdOrName);

        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Authorization", accessToken);

        // Request the latest sample for that sensor
        var body = new { sensors = new[] { sensorId }, limit = 1 };
        var samplesResp = await http.PostAsJsonAsync($"{_settings.BaseUrl}/samples", body);
        samplesResp.EnsureSuccessStatusCode();
        var json = await samplesResp.Content.ReadFromJsonAsync<JsonElement>();

        // { "sensors": { "<id>": [ { "observed": "...", "temperature": 73.67, ... } ] }, ... }
        var arr = json.GetProperty("sensors").GetProperty(sensorId);
        if (arr.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No samples returned for the sensor.");
        }

        var latest = arr[0];

        // Handle both shapes: number OR { "value": number }
        double tempF;
        var tempEl = latest.GetProperty("temperature");
        if (tempEl.ValueKind == JsonValueKind.Number)
        {
            tempF = tempEl.GetDouble();
        }
        else if (tempEl.ValueKind == JsonValueKind.Object && tempEl.TryGetProperty("value", out var vEl) && vEl.ValueKind == JsonValueKind.Number)
        {
            tempF = vEl.GetDouble();
        }
        else
        {
            throw new InvalidOperationException("Unexpected temperature payload shape.");
        }

        // Optional: sanity-check recency (ignore stale readings)
        var observed = latest.GetProperty("observed").GetDateTimeOffset();
        var age = DateTimeOffset.UtcNow - observed;
        if (age > TimeSpan.FromMinutes(15))
        {
            throw new InvalidOperationException($"Latest sample is stale ({age.TotalMinutes:F1} min old).");
        }

        return tempF; // already °F per your payload
    }
}
