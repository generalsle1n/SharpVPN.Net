using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SharpVPN.Net.GW;

public class IPv4Handler
{
    private readonly IConfiguration _config;
    private readonly ILogger<IPv4Handler> _logger;
    HashSet<IPv4RouteRecord> _routes = new HashSet<IPv4RouteRecord>();

    public IPv4Handler(IConfiguration Config, ILogger<IPv4Handler> Logger)
    {
        _config = Config;
        _logger = Logger;
        CreateRoutes();
    }

    private void CreateRoutes()
    {
        int Count = 0;
        while (true)
        {
            string Network = _config.GetValue<string>($"IPv4Routes:{Count}:Destination");
            if (Network is not null)
            {
                IPAddress Gateway = IPAddress.Parse(_config.GetValue<string>($"IPv4Routes:{Count}:Gateway"));
                int Subnet = int.Parse(_config.GetValue<string>($"IPv4Routes:{Count}:Subnet").Replace("/", ""));
                int Priorty = _config.GetValue<int>($"IPv4Routes:{Count}:Priority");
                IPNetwork IPNetwork = new IPNetwork(IPAddress.Parse(Network), Subnet);

                _routes.Add(new IPv4RouteRecord
                {
                    Destination = IPNetwork,
                    Gateway = Gateway,
                    Priorty = Priorty
                });
                _logger.LogInformation($"Route added: {IPNetwork} GW {Gateway}");
            }
            else
            {
                break;
            }

            Count++;
        }
        System.Console.WriteLine();
    }
}
