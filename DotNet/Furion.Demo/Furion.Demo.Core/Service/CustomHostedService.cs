using System.Threading;
using System.Threading.Tasks;
using Furion.EventBus;
using Microsoft.Extensions.Hosting;

namespace Furion.Demo.Core.Service;

public class CustomHostedService : IHostedService
{
    private readonly MySqlService _mySqlService;

    public CustomHostedService(MySqlService mySqlService)
    {
        _mySqlService = mySqlService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // MessageCenter.PublishAsync("XXX",)
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}