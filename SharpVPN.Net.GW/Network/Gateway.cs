using System.Data;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpVPN.Net.GW.Network.Entities;
using SharpVPN.Net.GW.Network.Entities.Implementations;
using SharpVPN.Net.GW.Network.Protocols;

namespace SharpVPN.Net.GW.Network;
public class Gateway : IHostedService
{
    private string _name;
    private readonly ILogger<Gateway> _logger;
    private readonly IConfiguration _config;
    private INetworkInterface _lan;
    private ARPHandler _arpHandler;
    
    public Gateway(IConfiguration Config, ILogger<Gateway> Logger, ARPHandler ARPHandler)
    {
        _config = Config;
        _logger = Logger;
        _arpHandler = ARPHandler;
        _name = _config.GetValue<string>("Gateway:Name");

        _lan = new PhysicalInterface()
        {
            Name = _config.GetValue<string>("Gateway:Interface:Name")
        };
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Gateway Started: {_name}");
        _arpHandler.GetMacByIP(IPAddress.Parse("172.16.1.1"));
        while (true)
        {
            _logger.LogDebug(DateTime.Now.ToString());
            await Task.Delay(1000);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}