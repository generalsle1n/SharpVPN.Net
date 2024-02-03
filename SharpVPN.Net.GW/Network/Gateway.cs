using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharpVPN.Net.GW.Network;
public class Gateway : IHostedService
{
    private string Name;
    private readonly ILogger<Gateway> Logger;
    private readonly IConfiguration Config;
    
    public Gateway(IConfiguration Config, ILogger<Gateway> Logger)
    {
        this.Config = Config;
        this.Logger = Logger;

        Name = this.Config.GetValue<string>("Gateway:Name");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            Logger.LogInformation(Name);
            await Task.Delay(1000);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}