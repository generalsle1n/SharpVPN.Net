using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
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
        
        _lan.EnableInterface(this);
        
        while (true)
        {
            _arpHandler.Resolve(_lan, IPAddress.Parse("182.15.5.6"));
            _logger.LogDebug(DateTime.Now.ToString());
            await Task.Delay(1000);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _lan.DisableInterface();
        return Task.CompletedTask;
    }

    public void PacketProcessor(object Sender, PacketCapture Caputre)
    {
        Packet Packet = Caputre.GetPacket().GetPacket();
        var a = Packet.Extract<ArpPacket>();
        _arpHandler.Resolve((INetworkInterface)Sender, IPAddress.Parse(""));
    }
}