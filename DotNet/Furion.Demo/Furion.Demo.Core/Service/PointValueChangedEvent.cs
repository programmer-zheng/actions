using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using Furion.DependencyInjection;
using Furion.EventBus;

namespace Furion.Demo.Core.Service;

public class PointValueChangedEvent : IEventSubscriber, ISingleton
{
    private readonly TdService _tdService;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent(TdService tdService)
    {
        _tdService = tdService;
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
    private readonly TdService _tdService;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent2(TdService tdService)
    {
        _tdService = tdService;
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
    private readonly TdService _tdService;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent3(TdService tdService)
    {
        _tdService = tdService;
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
    private readonly TdService _tdService;

    private ConcurrentDictionary<string, Channel<PointDataEntity>> _channels = new();

    public PointValueChangedEvent4(TdService tdService)
    {
        _tdService = tdService;
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
        _ = Task.Run(async () =>
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                await _tdService.InsertAndUpdate(item);
            }
        });
    }
}