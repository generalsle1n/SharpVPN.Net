using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpVPN.Net.GW.Network.Entities;

namespace SharpVPN.Net.GW;

public class ClientHandler
{
    private int _port { get; set; }
    private readonly ILogger<ClientHandler> _logger;
    private readonly IConfiguration _config;
    private TcpListener _listener;

    public ClientHandler(ILogger<ClientHandler> Logger, IConfiguration Config)
    {
        _logger = Logger;
        _config = Config;

        _port = _config.GetValue<int>("Gateway:Server:Port");
    }
    public void SetupServer(INetworkInterface Interface)
    {
        _listener = new TcpListener(new IPEndPoint(Interface.IPAddress, _port));
        _listener.Start();

        _logger.LogInformation($"Started Server {_listener.LocalEndpoint}");
    }
}
