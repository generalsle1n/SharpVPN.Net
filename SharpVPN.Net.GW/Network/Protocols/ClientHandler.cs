using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
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
    private X509Certificate _cert;
    private const string _certFileName = "server.crt";
    private const string _certKeyFileName = "server.key";

    public ClientHandler(ILogger<ClientHandler> Logger, IConfiguration Config)
    {
        _logger = Logger;
        _config = Config;

        _port = _config.GetValue<int>("Gateway:Server:Port");
        _cert = LoadCert();
    }
    public void SetupServer(INetworkInterface Interface)
    {
        _listener = new TcpListener(new IPEndPoint(Interface.IPAddress, _port));
        _listener.Start();
        _listener.BeginAcceptTcpClient(TCPClientHandler, _listener);
        _logger.LogInformation($"Started Server {_listener.LocalEndpoint}");
    }
    private void TCPClientHandler(IAsyncResult Result)
    {
        TcpClient Client = _listener.EndAcceptTcpClient(Result);
        using (SslStream Stream = new SslStream(Client.GetStream()))
        {
            Stream.AuthenticateAsServer(_cert, false, false);
            Stream.Read(Data);
        }
    }
    private X509Certificate LoadCert()
    {
        return new X509Certificate2("server.pfx", "password");
    }
}
