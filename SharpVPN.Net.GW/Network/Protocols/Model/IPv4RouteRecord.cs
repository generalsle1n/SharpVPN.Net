using System.Net;
using SharpPcap.LibPcap;

namespace SharpVPN.Net.GW;

public class IPv4RouteRecord
{
    internal IPNetwork2? Destination { get; init; }
    internal IPAddress? Gateway { get; init; }
    internal int Priorty { get; init; }
    internal LibPcapLiveDevice Interface { get; set; }
}
