using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SensorPull.Models.Configuration;
using SensorPull.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, cfg) =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false)
           .AddEnvironmentVariables();

#if DEBUG
        cfg.AddUserSecrets<Program>();
#endif
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureLogging(logging =>
    {
        // Ensure your categories are visible
        logging.SetMinimumLevel(LogLevel.Information);
        logging.AddFilter("Microsoft", LogLevel.Warning);
        logging.AddFilter("Worker", LogLevel.Information);

        // Replace these namespaces with YOUR real ones:
        logging.AddFilter("SensorPull.Functions.ThermostatTimer", LogLevel.Information);
        logging.AddFilter("SensorPull.Services.SensorPushClient", LogLevel.Information);
        logging.AddFilter("SensorPull.Services.GoveeClient", LogLevel.Information);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;
        services.AddSingleton(configuration);

        var sensorPushSettings = configuration.GetSection("SensorPushSettings").Get<SensorPushSettings>() ?? new SensorPushSettings();
        services.AddSingleton(sensorPushSettings);

        var goveeSettings = configuration.GetSection("GoveeSettings").Get<GoveeSettings>() ?? new GoveeSettings();
        services.AddSingleton(goveeSettings);

        services.AddHttpClient();
        services.AddSingleton<SensorPushClient>();
        services.AddSingleton<GoveeClient>();
    })
    .Build();

host.Run();
