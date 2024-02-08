using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpVPN.Net.GW.Network.Entities;
using SharpVPN.Net.GW.Network.Protocols.Model;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Timer = System.Timers.Timer;
using System.Reflection.Metadata;

namespace SharpVPN.Net.GW.Network.Protocols;

public class ARPHandler
{

    private PhysicalAddress _broadcast = PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
    private readonly ILogger<ARPHandler> _logger;
    private HashSet<ARPRecord> _record = new HashSet<ARPRecord>();
    private int _ttl;
    private Timer _timer;
    private IConfiguration _config;

    public ARPHandler(ILogger<ARPHandler> Logger, IConfiguration Config)
    {
        _logger = Logger;
        _config = Config;
        _ttl = _config.GetValue<int>("Gateway:ARP:TTL");
        _logger.LogDebug("ARP Loaded");
        SetupCleanup();
    }

    private void SetupCleanup()
    {
        _timer = new Timer();
        _timer.Interval = _ttl * 1000;
        _timer.Elapsed += CleanUpEntry;
        _timer.Enabled = true;
        _timer.Start();
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

    internal void AddARPRecord(ArpPacket Packet)
    {
        ARPRecord Record = new ARPRecord
        {
            IP = ((ArpPacket)Packet.PayloadPacket).SenderProtocolAddress,
            MAC = ((ArpPacket)Packet.PayloadPacket).SenderHardwareAddress,
            Created = DateTime.Now
        };

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
    private void CleanUpEntry(object sender, ElapsedEventArgs e)
    {
        DateTime Date = DateTime.Now;
        foreach (ARPRecord Record in _record)
        {
            int Seconds = (int)Math.Round((Date - Record.Created).TotalSeconds);
            if (Seconds >= _ttl)
            {
                _record.Remove(Record);
                _logger.LogDebug($"Removed ARP Entry: {Record.MAC}");
            }
        }
    }
}