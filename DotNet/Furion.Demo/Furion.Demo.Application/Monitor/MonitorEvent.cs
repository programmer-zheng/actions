using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Channels;

namespace Furion.Demo.Application.Monitor;

public class MonitorEvent : IEventSubscriber, ISingleton
{
    private readonly Channel<CustomMonitorEventDto> _channel;
    public MonitorEvent(IServiceProvider serviceProvider)
    {
        _channel = serviceProvider.GetKeyedService<Channel<CustomMonitorEventDto>>("Monitor");
    }

    [EventSubscribe("Monitor_Event")]
    public async Task HandleEvent(EventHandlerExecutingContext context)
    {
        var data = context.GetPayload<CustomMonitorEventDto>();
        var dataStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        await _channel.Writer.WriteAsync(data);
    }

}
