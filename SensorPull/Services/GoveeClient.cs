using SensorPull.Models.Configuration;
using SensorPull.Models.Govee;
using System.Net.Http.Json;
using System.Text.Json;

namespace SensorPull.Services;

public class GoveeClient(IHttpClientFactory httpFactory, GoveeSettings settings)
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly GoveeSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public async Task<Response> ListDevices()
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Govee-API-Key", _settings.ApiKey ?? throw new InvalidOperationException("API Key is not set in GoveeSettings."));
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            throw new InvalidOperationException("BaseUrl is not set in GoveeSettings.");
        }

        var resp = await http.GetAsync($"{_settings.BaseUrl}/router/api/v1/user/devices");
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<Response>(json);
        return response ?? throw new InvalidOperationException("Failed to deserialize Govee response.");
    }

    public async Task<GoveeSwitchStatus> GetSwitchState(string deviceId, string sku)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Govee-API-Key", _settings.ApiKey ?? throw new InvalidOperationException("API Key is not set in GoveeSettings."));
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            throw new InvalidOperationException("BaseUrl is not set in GoveeSettings.");
        }

        var resp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/router/api/v1/device/state",
            new DeviceStateRequest
            {
                RequestId = Guid.NewGuid().ToString(),
                Payload = new DeviceStateRequestPayload
                {
                    Device = deviceId,
                    Sku = sku
                }
            });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var state = JsonSerializer.Deserialize<DeviceStateResponse>(json);
        if (state == null)
        {
            throw new InvalidOperationException("Failed to deserialize Govee response.");
        }

        bool online = false;
        bool isOn = false;

        if (state.Payload != null)
        {
            foreach (var cap in state!.Payload!.Capabilities!)
            {
                if (cap.Type == "devices.capabilities.online")
                {
                    if (cap.State.Value is JsonElement je)
                    {
                        if (je.ValueKind == JsonValueKind.True) online = true;
                        else if (je.ValueKind == JsonValueKind.False) online = false;
                        else if (je.ValueKind == JsonValueKind.Number) online = je.GetInt32() == 1;
                    }
                }
                else if (cap.Type == "devices.capabilities.on_off")
                {
                    if (cap.State.Value is JsonElement je)
                    {
                        if (je.ValueKind == JsonValueKind.Number)
                        {
                            isOn = je.GetInt32() == 1;
                        }
                        else if (je.ValueKind == JsonValueKind.True)
                        {
                            isOn = true;
                        }
                        else if (je.ValueKind == JsonValueKind.False)
                        {
                            isOn = false;
                        }
                    }
                }
            }
        }

        return new GoveeSwitchStatus { Online = online, IsOn = isOn };
    }

    public async Task<ToggleSwitchResponse> TurnHeatPad(bool on)
    {
        var http = _httpFactory.CreateClient();
        http.DefaultRequestHeaders.Add("Govee-API-Key", _settings.ApiKey ?? throw new InvalidOperationException("API Key is not set in GoveeSettings."));
        if (string.IsNullOrEmpty(_settings.BaseUrl))
        {
            throw new InvalidOperationException("BaseUrl is not set in GoveeSettings.");
        }

        var resp = await http.PostAsJsonAsync(
            $"{_settings.BaseUrl}/router/api/v1/device/control",
            new UpdateDeviceStatus
            {
                RequestId = Guid.NewGuid().ToString(),
                Payload = new UpdateDevicePayload
                {
                    Device = _settings.HeatPadSmartPlugDeviceId ?? throw new InvalidOperationException("HeatPadSmartPlugDeviceId is not set in GoveeSettings."),
                    Sku = _settings.HeadPadSmartPlugSku ?? throw new InvalidOperationException("HeadPadSmartPlugSku is not set in GoveeSettings."),
                    Capability = new UpdateDeviceCapability
                    {
                        Type = "devices.capabilities.on_off",
                        Instance = "powerSwitch",
                        Value = on ? 1 : 0
                    }
                }
            });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ToggleSwitchResponse>(json);
        return response ?? throw new InvalidOperationException("Failed to deserialize Govee response.");
    }
}
