using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Authentication;

namespace SharpVPN.Net.Client.Network;

public class ServerConnectionHandler
{
    private readonly IConfiguration _config;
    private readonly ILogger<ServerConnectionHandler> _logger;
    private string _server { get; set; }
    private int _serverPort { get; set; }
    private string _serverThumbprint { get; set; }

    private TcpClient _client { get; set; }
    private SslStream _stream { get; set; }
    public ServerConnectionHandler(IConfiguration Config, ILogger<ServerConnectionHandler> Logger)
    {
        _config = Config;
        _logger = Logger;

        _server = _config.GetValue<string>("Server:IP");
        _serverPort = _config.GetValue<int>("Server:Port");
        _serverThumbprint = _config.GetValue<string>("Server:Thumbprint");
        TryConnectToServer();
    }

    public bool TryConnectToServer()
    {
        bool Result = false;

        _client = new TcpClient();
        _client.Connect(_server, _serverPort);
        Stream ClientStream = _client.GetStream();
        _stream = new SslStream(ClientStream, false, CertCheck, null);

        try
        {
            _logger.LogInformation($"Try to Connect to Server with Cert Thumbprint: {_serverThumbprint}");
            _stream.AuthenticateAsClient("");
            Result = true;
        }
        catch (AuthenticationException Auth)
        {
            _logger.LogCritical($"Cannot connect to server via Secure Stream, the Server had an diffrent Thumbprint than defined");
        }

        return Result;
    }
    private bool CertCheck(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        X509Certificate2 Cert = new X509Certificate2(certificate);
        if (Cert.Thumbprint.Equals(_serverThumbprint))
        {
            _logger.LogInformation("Server Cert Succes");
            return true;
        }
        else
        {
            _logger.LogError($"Cert Check failed, Presented Server Cert: {Cert.Thumbprint} and Configured: {_serverThumbprint}");
            return false;
        }
    }
}
