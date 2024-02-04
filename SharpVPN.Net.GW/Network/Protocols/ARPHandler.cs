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

    internal void AddARPRecord(ARPRecord Record)
    {
        if (!IPIsKnown(Record))
        {
            _record.Add(Record);
            _logger.LogDebug($"ARP Added {Record.IP}");
        }
    }

    private bool IPIsKnown(ARPRecord Record)
    {
        bool Result = false;
        foreach (ARPRecord Single in _record)
        {
            if (Single.IP.Equals(Record.IP))
            {
                Result = true;
                break;
            }
        }

        return Result;
    }
    
    internal void Resolve(INetworkInterface Interface, IPAddress Destination)
    {
        LibPcapLiveDevice Device = LibPcapLiveDeviceList.New().Where(x => x.Name.Equals(Interface.Name)).First();
        ArpPacket RequestArp = new ArpPacket(
            ArpOperation.Request, 
            _broadcast, 
            Destination, 
            Device.MacAddress,
            Device.Addresses[0].Addr.ipAddress
        );

        EthernetPacket Packet = new EthernetPacket(Device.MacAddress, _broadcast, EthernetType.Arp)
        {
            PayloadPacket = RequestArp
        };
        Device.SendPacket(Packet);
        
    }
}