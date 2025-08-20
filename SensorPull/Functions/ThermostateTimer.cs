using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SensorPull.Models.Configuration;
using SensorPull.Services;

namespace SensorPull.Functions;

public class ThermostatTimer(ILoggerFactory loggerFactory,
    SensorPushSettings sensorPushSettings,
    GoveeSettings goveeSetttings,
    SensorPushClient sensorPush,
    GoveeClient goveeClient)
{
    private readonly ILogger _log = loggerFactory.CreateLogger<ThermostatTimer>();
    private readonly SensorPushSettings _sensorPushSettings = sensorPushSettings;
    private readonly GoveeSettings _goveeSettings = goveeSetttings;
    private readonly SensorPushClient _sensorPushClient = sensorPush;
    private readonly GoveeClient _goveeClient = goveeClient;

    // Runs every 2 minutes
    [Function("ThermostatTimer")]
    public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
    {
        _log.LogWarning("######## ThermostatTimer started at {Now:o} ########", DateTimeOffset.UtcNow);
        try
        {
            var (minDegrees, maxDegrees) = await _sensorPushClient.GetTemperatureAlertThresholdsAsync(_sensorPushSettings.SensorIdOrName!);

            // 1) Read SensorPush temperature (°F)
            var tempF = await _sensorPushClient.GetLatestTemperatureFAsync(_sensorPushSettings.SensorIdOrName!);
            _log.LogInformation("Current temp: {tempF:F2}°F (range {low}-{high})", tempF, minDegrees, maxDegrees);

            // 2) Decide desired switch state (ON = provide heat)
            //    If below LOW -> ON, if above HIGH -> OFF, else keep current
            var heatPadState = await _goveeClient.GetSwitchState(_goveeSettings.HeatPadSmartPlugDeviceId!, _goveeSettings.HeadPadSmartPlugSku!);
            
            if (!heatPadState.Online)
            {
                _log.LogWarning("HeatPad smart plug is offline; cannot change state.");
                return;
            }
            _log.LogInformation("HeatPad switch is currently {state}", heatPadState.IsOn ? "ON" : "OFF");

            bool? desiredOn = null;

            if (tempF < minDegrees)
            {
                desiredOn = true;
            }
            else if (tempF > maxDegrees)
            {
                desiredOn = false;
            }

            if (desiredOn.HasValue)
            {
                if (desiredOn.Value != heatPadState.IsOn)
                {
                    await _goveeClient.TurnHeatPad(desiredOn.Value);
                    _log.LogInformation("Switch changed to {state}", desiredOn.Value ? "ON" : "OFF");
                }
                else
                {
                    _log.LogInformation("Switch already {state}; no change.", heatPadState.IsOn ? "ON" : "OFF");
                }
            }
            else
            {
                _log.LogInformation("Temp within band; leaving switch {state}.", heatPadState.IsOn ? "ON" : "OFF");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ThermostatTimer failed.");
            throw;
        }
    }
}
