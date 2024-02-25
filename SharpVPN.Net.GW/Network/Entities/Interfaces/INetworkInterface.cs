using System.Net;

namespace SharpVPN.Net.GW.Network.Entities;

public interface INetworkInterface
{
    public string Name { get; }
    public void EnableInterface(Gateway Gateway);
    public void DisableInterface();
    public void SetIPAddress(IPAddress IP, string Subnet);
    public IPAddress IPAddress { get; set; }
}