using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Furion.Demo.Application.Monitor;

public class MonitorEvent : IEventSubscriber, ISingleton
{
    private readonly Channel<CustomMonitorEventDto> _channel;
    private readonly ILogger<MonitorEvent> _logger;

    public MonitorEvent(IServiceProvider serviceProvider, ILogger<MonitorEvent> logger)
    {
        _channel = serviceProvider.GetKeyedService<Channel<CustomMonitorEventDto>>("Monitor");
        _logger = logger;
    }

    [EventSubscribe("Monitor_Event")]
    public async Task HandleEvent(EventHandlerExecutingContext context)
    {
        var data = context.GetPayload<CustomMonitorEventDto>();
        await _channel.Writer.WriteAsync(data);
        _logger.LogInformation("事件数据已写入Channel: {Data}", JsonConvert.SerializeObject(data));
    }
}
