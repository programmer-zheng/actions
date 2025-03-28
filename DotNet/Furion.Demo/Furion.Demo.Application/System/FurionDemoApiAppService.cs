using Furion.Demo.Application.System.Dtos;
using Furion.EventBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Furion.Demo.Application.System;

[ApiDescriptionSettings("Furion CustomGroup", Description = "自定义分组")]
[Route("api/FurionDemoApi")]
public class FurionDemoApiAppService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Channel<string> _channel;

    private readonly IEventPublisher _eventPublisher;

    public FurionDemoApiAppService(IHttpContextAccessor contextAccessor, IServiceProvider serviceProvider, IEventPublisher eventPublisher)
    {
        _contextAccessor = contextAccessor;
        _channel = serviceProvider.GetKeyedService<Channel<string>>("Monitor");
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// 打招呼
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public string SayHello()
    {
        return "Hello Furion";
    }

    [HttpGet]
    public async Task<CustomMonitorEventDto> SendMessage()
    {
        var obj = new CustomMonitorEventDto { Id = Random.Shared.Next(10000, 99999), Msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        await _eventPublisher.PublishAsync("Monitor_Event", obj);
        return obj;
    }

    [ActionName("ServerSendEvent")]
    [HttpGet]
    public async Task ServerSendEventAsync()
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }
        httpContext.Response.Headers.Add("Content-Type", "text/event-stream");
        httpContext.Response.Headers.Add("Cache-Control", "no-cache");
        httpContext.Response.Headers.Add("Connection", "keep-alive");
        try
        {
            await _channel.Writer.WriteAsync("data: \n");
            //_ = Task.Run(async () =>
            //{
            //    while (!httpContext.RequestAborted.IsCancellationRequested)
            //    {

            //        await _channel.Writer.WriteAsync("data: 200 from task\n\n").ConfigureAwait(false);
            //        await Task.Delay(1000);
            //    }
            //});
            await foreach (var message in _channel.Reader.ReadAllAsync())
            {
                if (httpContext.RequestAborted.IsCancellationRequested)
                {
                    break;
                }
                var dataPage = Encoding.UTF8.GetBytes(message);
                await httpContext.Response.Body.WriteAsync(dataPage, 0, dataPage.Length);
                await httpContext.Response.Body.FlushAsync();
                //await _channel.Writer.WriteAsync(message);
            }
        }
        catch (OperationCanceledException oce)
        {
            Log.Error("用户取消了");
        }
        catch (Exception ex)
        {
            Log.Error("响应sse出错", ex);
        }
    }
}
