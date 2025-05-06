using System.Threading.Tasks;
using Furion.DependencyInjection;
using Furion.EventBus;

namespace Furion.Demo.Core.Service;

public class PointValueChangedEvent : IEventSubscriber, ISingleton
{
    private readonly TdService _tdService;

    public PointValueChangedEvent(TdService tdService)
    {
        _tdService = tdService;
    }


    [EventSubscribe("PointValueChanged")]
    public async Task Handler(EventHandlerExecutingContext context)
    {
        var eto = context.GetPayload<PointDataEntity>();
        await _tdService.InsertAndUpdate(eto);
    }
}