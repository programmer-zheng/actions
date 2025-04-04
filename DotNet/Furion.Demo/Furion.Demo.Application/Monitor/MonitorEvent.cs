using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Furion.Demo.Application.Monitor;

public class MonitorEvent : IEventSubscriber, ISingleton
{
    private readonly Channel<string> _channel;
    public MonitorEvent(IServiceProvider serviceProvider)
    {
        _channel = serviceProvider.GetKeyedService<Channel<string>>("Monitor");
    }

    [EventSubscribe("Monitor_Event")]
    public async Task HandleEvent(EventHandlerExecutingContext context)
    {
        var data = context.GetPayload<CustomMonitorEventDto>();
        var dataStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
        await _channel.Writer.WriteAsync($"data: {dataStr}\n\n");
    }

}
