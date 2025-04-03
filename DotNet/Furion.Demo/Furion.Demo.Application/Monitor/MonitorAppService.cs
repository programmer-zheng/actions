using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Furion.Demo.Application.Monitor;

public class MonitorAppService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Channel<string> _channel;

    private readonly IEventPublisher _eventPublisher;

    private List<PointRealTimeDataDto> _pointRealTimeData;
    private List<StationMonitorDto> _stationMonitorData;

    public MonitorAppService(IHttpContextAccessor contextAccessor, IServiceProvider serviceProvider, IEventPublisher eventPublisher)
    {
        _contextAccessor = contextAccessor;
        _channel = serviceProvider.GetKeyedService<Channel<string>>("Monitor");
        _eventPublisher = eventPublisher;

        InitData();
    }

    private void InitData()
    {
        _pointRealTimeData = new()
        {
            new (){ PointNumber="153A03",PointName=" 高低浓甲烷", InstallAddress="安装位置", PointValue="0.02%", Status="正常" },
            new (){ PointNumber="153A04",PointName=" 瓦斯", InstallAddress="安装位置", PointValue="0.01%", Status="正常" }
        };

        _stationMonitorData = new()
        {
            new(){ Sno="152", Model="24型", Address="安装位置", BatteryVoltage="27.6v", Status="正常" },
            new(){ Sno="153", Model="16型", Address="安装位置", BatteryVoltage="27.6v", Status="正常" },
        };
    }

    /// <summary>
    /// 测点数据变更
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task PointChangeAsync()
    {
        var data = new List<PointRealTimeDataDto>()
        {
            new (){ PointNumber="153A04",PointName=" 瓦斯", InstallAddress="安装位置", PointValue= $"{Random.Shared.Next(1,3)}%", Status="异常" }
        };
        var obj = new CustomMonitorEventDto(MonitorEventType.Point, data);
        await _eventPublisher.PublishAsync("Monitor_Event", obj);
    }

    /// <summary>
    /// 分站数据变更
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task StationChangeAsync()
    {

        var data = new List<StationMonitorDto>()
        {
            new(){ Sno="152", Model="24型", Address="安装位置", BatteryVoltage=$"{Random.Shared.Next(20,30)}v", Status="正常" },
        };
        var obj = new CustomMonitorEventDto(MonitorEventType.Station, data);
        await _eventPublisher.PublishAsync("Monitor_Event", obj);
    }

    /// <summary>
    /// 获取测点实时数据
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public object GetPointRealTimeData()
    {
        return _pointRealTimeData;
    }

    /// <summary>
    /// 获取分站数据
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public object GetStationData()
    {
        return _stationMonitorData;
    }

    /// <summary>
    /// sse
    /// </summary>
    /// <returns></returns>
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

            //        await _channel.Writer.WriteAsync("data: \n");
            //        await Task.Delay(1000 * 30);
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
