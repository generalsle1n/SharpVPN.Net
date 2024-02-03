// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SharpVPN.Net.GW.Network;
using ILogger = Serilog.ILogger;

const string SettingsFileName = "settings.json";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
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
        service.AddHostedService<Gateway>();
    });

IHost App = Builder.Build();

await App.RunAsync();