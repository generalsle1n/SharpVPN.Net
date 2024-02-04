using System.Net;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpVPN.Net.GW.Network.Protocols;

namespace SharpVPN.Net.GW.Network.Entities;

public interface INetworkInterface
{
    public string Name { get; }
    public void EnableInterface(Gateway Gateway);
    public void DisableInterface();
}