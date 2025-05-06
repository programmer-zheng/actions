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
        Channel<PointDataEntity> channel;
        if (_channels.TryGetValue(sno, out var _channel))
        {
            channel = _channel;
        }
        else
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