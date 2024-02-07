using System.Net;
using System.Net.NetworkInformation;

namespace SharpVPN.Net.GW.Network.Protocols.Model;

public class ARPRecord
{
    internal PhysicalAddress MAC { get; set; }
    internal IPAddress IP { get; set; }
    internal DateTime Created { get; set; }
}