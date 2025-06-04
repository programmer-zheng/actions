using System;
using System.Threading;
using System.Threading.Tasks;
using Furion.EventBus;
using Microsoft.Extensions.Hosting;

namespace Furion.Demo.Core.Service;

public class CustomHostedService(GlobalCollectionService globalCollectionService) : IHostedService
{
    private CancellationTokenSource _cancellationTokenSource;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        // 确保后台任务在启动后异步执行
        _ = Task.Run(async () =>
        {
            await RunAllAsyncInBackground(_cancellationTokenSource.Token);
        }, cancellationToken);
        return Task.CompletedTask;
    }

    private async Task RunAllAsyncInBackground(CancellationToken token)
    {
        await globalCollectionService.InitializeAsync();
        await globalCollectionService.RunAllAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}