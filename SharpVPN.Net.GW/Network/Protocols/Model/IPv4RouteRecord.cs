using System.Net;

namespace SharpVPN.Net.GW;

public class IPv4RouteRecord
{
    internal IPNetwork? Destination { get; init; }
    internal IPAddress? Gateway { get; init; }
    internal int Priorty { get; init; }
}
