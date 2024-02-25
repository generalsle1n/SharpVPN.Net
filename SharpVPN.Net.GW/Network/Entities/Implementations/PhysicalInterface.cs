using SharpPcap;
using SharpPcap.LibPcap;
using System.Net;


namespace SharpVPN.Net.GW.Network.Entities.Implementations;

public class PhysicalInterface : INetworkInterface
{
    public string Name { get; init; }
    private LibPcapLiveDevice _interface { get; set; }
    public IPAddress IPAddress { get; set; }

    public void EnableInterface(Gateway Gateway)
    {
        _interface = LibPcapLiveDeviceList.New().Where(x => x.Name.Equals(Name)).First();
        _interface.OnPacketArrival += Gateway.PacketProcessor;
        _interface.Open();
        _interface.StartCapture();
    }
    public void DisableInterface()
    {
        _interface.StopCapture();
        _interface.Close();
        _interface.Dispose();
    }

    public void SetIPAddress(IPAddress IP, string Subnet)
    {
        IPAddress = IP;
    }
}