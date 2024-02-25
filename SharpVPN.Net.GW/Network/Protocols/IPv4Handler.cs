using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace SharpVPN.Net.GW;

public class IPv4Handler
{
    private readonly IConfiguration _config;
    private readonly ILogger<IPv4Handler> _logger;
    HashSet<IPv4RouteRecord> _routes = new HashSet<IPv4RouteRecord>();

    public IPv4Handler(IConfiguration Config, ILogger<IPv4Handler> Logger)
    {
        _config = Config;
        _logger = Logger;
        CreateRoutes();
    }

    public void Process(IPPacket Packet)
    {
        IPv4RouteRecord Route = GetRoute(Packet);
        if (Route is IPv4RouteRecord)
        {
            _logger.LogDebug($"{Packet.DestinationAddress} is routed");
            //Send Packet
        }
        else
        {
            _logger.LogDebug($"{Packet.DestinationAddress} is not routed");
        }
    }

    private void CreateRoutes()
    {
        int Count = 0;
        while (true)
        {
            string Network = _config.GetValue<string>($"IPv4Routes:{Count}:Destination");
            if (Network is not null)
            {
                IPAddress Gateway = IPAddress.Parse(_config.GetValue<string>($"IPv4Routes:{Count}:Gateway"));
                int Subnet = int.Parse(_config.GetValue<string>($"IPv4Routes:{Count}:Subnet").Replace("/", ""));
                int Priorty = _config.GetValue<int>($"IPv4Routes:{Count}:Priority");
                IPNetwork2 IPNetwork = IPNetwork2.Parse($"{Gateway}/{Subnet}");

                LibPcapLiveDevice Device = GetDeviceFromInterfaceName(_config.GetValue<string>($"IPv4Routes:{Count}:Interface"));
                _routes.Add(new IPv4RouteRecord
                {
                    Destination = IPNetwork,
                    Gateway = Gateway,
                    Priorty = Priorty,
                    Interface = Device
                });
                _logger.LogInformation($"Route added: {IPNetwork} GW {Gateway} Interface: {Device.Name}");
            }
            else
            {
                break;
            }

            Count++;
        }
    }
    private LibPcapLiveDevice GetDeviceFromIP(IPAddress IP)
    {
        LibPcapLiveDeviceList AllDevices = LibPcapLiveDeviceList.Instance;

        LibPcapLiveDevice Result = null;

        foreach (LibPcapLiveDevice Device in AllDevices)
        {
            foreach (PcapAddress Address in Device.Addresses)
            {
                if (Address.Addr.ipAddress is not null)
                {
                    if (Address.Addr.ipAddress.Equals(IP))
                    {
                        Result = Device;
                        break;
                    }
                }
            }
            if (Result is not null)
            {
                break;
            }
        }
        return Result;
    }
    private LibPcapLiveDevice GetDeviceFromInterfaceName(string Name)
    {
        return LibPcapLiveDeviceList.Instance.Where(inter =>
        {
            if (inter.Name.Equals(Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }).FirstOrDefault();
    }
    private LibPcapLiveDevice GetNetworkdeviceFromRoute(IPAddress Destination)
    {
        IPv4RouteRecord Result = _routes.Where(route =>
        {
            return route.Destination.Contains(Destination);
        }).First();
        return Result.Interface;
    }
    private IPv4RouteRecord GetRoute(IPPacket IP)
    {
        IPv4RouteRecord Result = null;
        foreach (IPv4RouteRecord Route in _routes)
        {
            if (Route.Destination.Contains(IP.DestinationAddress))
            {
                Result = Route;
                break;
            };
        }
        return Result;
    }
}
