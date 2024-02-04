using SharpVPN.Net.GW.Network.Protocols;

namespace SharpVPN.Net.GW.Network.Entities.Implementations;

public class PhysicalInterface : INetworkInterface
{
    public string Name { get; init; }
}