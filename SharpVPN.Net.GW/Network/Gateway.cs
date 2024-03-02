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
    private IPv4Handler _ipv4Handler;
    private List<INetworkInterface> _interfaces = new List<INetworkInterface>();
    private ClientHandler _client;

    public Gateway(IConfiguration Config, ILogger<Gateway> Logger, ARPHandler ARPHandler, IPv4Handler IPv4Hanlder, ClientHandler Client)
    {
        _config = Config;
        _logger = Logger;
        _arpHandler = ARPHandler;
        _ipv4Handler = IPv4Hanlder;
        _name = _config.GetValue<string>("Gateway:Name");
        _client = Client;

        SetupInterfaces();
        _client.SetupServer(_interfaces[0]);
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Gateway Started: {_name}");
        while (true)
        {
            _logger.LogDebug(DateTime.Now.ToString());
            await Task.Delay(10000);
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
        switch (Packet.PayloadPacket)
        {
            case ArpPacket:
                _logger.LogDebug("Incoming Arp Response");
                if (((ArpPacket)Packet.PayloadPacket).Operation == ArpOperation.Response)
                {
                    _arpHandler.AddARPRecord((ArpPacket)Packet.PayloadPacket);
                }
                break;
            case IPPacket:
                if (Packet.PayloadPacket is IPv4Packet)
                {
                    _ipv4Handler.Process((IPPacket)Packet.PayloadPacket);
                }
                else
                {
                    _logger.LogDebug("Discarded IPv6 Packet");
                }

                break;
        }
    }
    private void SetupInterfaces()
    {
        int Count = 0;
        while (true)
        {
            string Name = _config.GetValue<string>($"Gateway:Interface:{Count}:Name");
            if (Name is not null)
            {
                IPAddress IP = IPAddress.Parse(_config.GetValue<string>($"Gateway:Interface:{Count}:IP"));
                string Type = _config.GetValue<string>($"Gateway:Interface:{Count}:Type");
                string Subnet = _config.GetValue<string>($"Gateway:Interface:{Count}:Subnet");

                if (Type.Equals("Physical"))
                {
                    INetworkInterface Interface = new PhysicalInterface
                    {
                        Name = Name
                    };
                    Interface.EnableInterface(this);
                    Interface.SetIPAddress(IP, Subnet);
                    _interfaces.Add(Interface);

                    _logger.LogInformation($"Added {Type} Interface {Name} and Enabled");
                }
                else if (Type.Equals("Virtual"))
                {
                    INetworkInterface Interface = new VirtualInterface()
                    {
                        Name = Name,
                        IPAddress = IP
                    };
                    Interface.EnableInterface(this);
                    Interface.SetIPAddress(IP, Subnet);
                    _interfaces.Add(Interface);
                    _logger.LogInformation($"Added {Type} Interface {Name} and Enabled");
                }
            }
            else
            {
                break;
            }

            Count++;
        }
    }
}