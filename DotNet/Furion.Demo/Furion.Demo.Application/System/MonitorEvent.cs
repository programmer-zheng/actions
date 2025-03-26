using Furion.Demo.Application.System.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System;

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
        var source = context.Source;
        var data = context.GetPayload<CustomMonitorEventDto>();
        await _channel.Writer.WriteAsync($"data: {JsonConvert.SerializeObject(data)} from event \n\n").ConfigureAwait(false);
    }
    
}
