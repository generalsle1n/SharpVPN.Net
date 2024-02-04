using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpVPN.Net.GW.Network.Entities;
using SharpVPN.Net.GW.Network.Protocols.Model;

namespace SharpVPN.Net.GW.Network.Protocols;

public class ARPHandler
{

    private PhysicalAddress _broadcast = PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
    private readonly ILogger<ARPHandler> _logger;
    private HashSet<ARPRecord> _record = new HashSet<ARPRecord>();

    public ARPHandler(ILogger<ARPHandler> Logger)
    {
        _logger = Logger;
        _logger.LogDebug("ARP Loaded");
        var a =IPAddress.Parse("172.16.1.1");
        var re = new ARPRecord()
        {
            IP = a,
            MAC = _broadcast,
            TTL = 60
        };
        _record.Add(re);
    }

    internal ARPRecord GetMacByIP(IPAddress IP)
    {
        _logger.LogDebug($"Search MAC for IP {IP}");
        ARPRecord Result = null;
        foreach (ARPRecord Single in _record)
        {
            if (Single.IP.Equals(IP))
            {
                Result = Single;
                _logger.LogDebug($"Found MAC {Single.MAC} for {IP}");
                break;
            }
        }

        return Result;
    }
    
    
    // internal void Resolve(INetworkInterface Interface)
    // {
    //     LibPcapLiveDevice Correct = LibPcapLiveDeviceList.New().Where(LocalInt =>
    //     {
    //         if (LocalInt.Name.Equals(Interface.Name))
    //         {
    //             return true;
    //         }
    //         else
    //         {
    //             return false;
    //         }
    //
    //     }).First();
    //     IPAddress destination = IPAddress.Parse("192.168.1.1");
    //     IPAddress source = IPAddress.Parse("192.168.1.196");
    //     var acc = new ArpPacket(ArpOperation.Request, _broadcast, destination, Correct.MacAddress, source);
    //     // ArpPacket a = new ArpPacket(ArpOperation.Request, _broadcast, ac, Correct.MacAddress, ac);
    //     EthernetPacket c = new EthernetPacket(Correct.MacAddress, _broadcast, EthernetType.None)
    //     {
    //         PayloadPacket = acc
    //     };
    //     Correct.Open();
    //     Correct.SendPacket(c);
    // }
}