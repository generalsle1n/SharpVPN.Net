using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpVPN.Net.Client.Network;

namespace SharpVPN.Net.Client;

public class Gateway : IHostedService
{
    private readonly IConfiguration _config;
    private readonly ILogger<Gateway> _logger;
    private ServerConnectionHandler _server;
    public Gateway(IConfiguration Config, ILogger<Gateway> Logger, ServerConnectionHandler Server)
    {
        _config = Config;
        _logger = Logger;
        _server = Server;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
