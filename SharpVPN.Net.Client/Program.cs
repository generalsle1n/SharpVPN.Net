using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SharpVPN.Net.Client;
using SharpVPN.Net.Client.Network;

const string SettingsFileName = "settings.json";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

var c = new ConfigurationBuilder()
    .AddJsonFile(SettingsFileName, false, true)
    .Build();

var Builder = new HostBuilder()
    .UseSerilog()
    .ConfigureAppConfiguration((config) =>
    {
        config.AddJsonFile(SettingsFileName, false, true);
    })
    .ConfigureServices((service) =>
    {
        service.AddSingleton<ServerConnectionHandler>();
        service.AddHostedService<Gateway>();
    });

IHost App = Builder.Build();

await App.RunAsync();
