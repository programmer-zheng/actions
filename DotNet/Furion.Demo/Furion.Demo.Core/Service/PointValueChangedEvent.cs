using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using Furion.DependencyInjection;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace Furion.Demo.Core.Service;

public class PointValueChangedEvent : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }


    [EventSubscribe("PointValueChanged")]
    public async Task Handler(EventHandlerExecutingContext context)
    {
        var eto = context.GetPayload<PointDataEntity>();
        var sno = eto.SNO;
        if (!_channels.TryGetValue(sno, out var channel))
        {
            channel = Channel.CreateUnbounded<PointDataEntity>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            _channels.TryAdd(sno, channel);
            xxx(channel.Reader);
        }

        channel.Writer.TryWrite(eto);
    }

    void xxx(ChannelReader<PointDataEntity> reader)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var _tdService = serviceScope.ServiceProvider.GetService<TdService>();
        _ = Task.Run(async () =>
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                await _tdService.InsertAndUpdate(item);
            }
        });
    }
}

public class PointValueChangedEvent2 : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent2(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }


    [EventSubscribe("PointValueChanged2")]
    public async Task Handler(EventHandlerExecutingContext context)
    {
        var eto = context.GetPayload<PointDataEntity>();
        var sno = eto.SNO;
        if (!_channels.TryGetValue(sno, out var channel))
        {
            channel = Channel.CreateUnbounded<PointDataEntity>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            _channels.TryAdd(sno, channel);
            xxx(channel.Reader);
        }

        channel.Writer.TryWrite(eto);
    }

    void xxx(ChannelReader<PointDataEntity> reader)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var _tdService = serviceScope.ServiceProvider.GetService<TdService>();
        _ = Task.Run(async () =>
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                await _tdService.InsertAndUpdate(item);
            }
        });
    }
}

public class PointValueChangedEvent3 : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent3(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }


    [EventSubscribe("PointValueChanged3")]
    public async Task Handler(EventHandlerExecutingContext context)
    {
        var eto = context.GetPayload<PointDataEntity>();
        var sno = eto.SNO;
        if (!_channels.TryGetValue(sno, out var channel))
        {
            channel = Channel.CreateUnbounded<PointDataEntity>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            _channels.TryAdd(sno, channel);
            xxx(channel.Reader);
        }

        channel.Writer.TryWrite(eto);
    }

    void xxx(ChannelReader<PointDataEntity> reader)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var _tdService = serviceScope.ServiceProvider.GetService<TdService>();
        _ = Task.Run(async () =>
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                await _tdService.InsertAndUpdate(item);
            }
        });
    }
}

public class PointValueChangedEvent4 : IEventSubscriber, ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent4(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }


    [EventSubscribe("PointValueChanged4")]
    public async Task Handler(EventHandlerExecutingContext context)
    {
        var eto = context.GetPayload<PointDataEntity>();
        var sno = eto.SNO;
        if (!_channels.TryGetValue(sno, out var channel))
        {
            channel = Channel.CreateUnbounded<PointDataEntity>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            _channels.TryAdd(sno, channel);
            xxx(channel.Reader);
        }

        channel.Writer.TryWrite(eto);
    }

    void xxx(ChannelReader<PointDataEntity> reader)
    {
        using var serviceScope = _scopeFactory.CreateScope();

        var _tdService = serviceScope.ServiceProvider.GetService<TdService>();
        _ = Task.Run(async () =>
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                await _tdService.InsertAndUpdate(item);
            }
        });
    }
}