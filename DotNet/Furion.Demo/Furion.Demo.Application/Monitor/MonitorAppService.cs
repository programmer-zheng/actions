using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Threading.Channels;
using System.Threading;

namespace Furion.Demo.Application.Monitor;

public class MonitorAppService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Channel<CustomMonitorEventDto> _channel;

    private readonly IEventPublisher _eventPublisher;

    private List<PointRealTimeDataDto> _pointRealTimeData;
    private List<StationMonitorDto> _stationMonitorData;

    public MonitorAppService(IHttpContextAccessor contextAccessor, IServiceProvider serviceProvider, IEventPublisher eventPublisher)
    {
        _contextAccessor = contextAccessor;
        _channel = serviceProvider.GetKeyedService<Channel<CustomMonitorEventDto>>("Monitor");
        _eventPublisher = eventPublisher;

        InitData();
    }

    private void InitData()
    {
        _pointRealTimeData = new()
        {
            new (){ Id=1234, PointNumber="153A03",PointName=" 高低浓甲烷", InstallAddress="安装位置", PointValue="0.02%", Status="正常" },
            new (){ Id=5678, PointNumber="153A04",PointName=" 瓦斯", InstallAddress="安装位置", PointValue="0.01%", Status="正常" }
        };

        _stationMonitorData = new()
        {
            new(){ Id=1357, Sno="152", Model="24型", Address="安装位置", BatteryVoltage="27.6v", Status="正常" },
            new(){ Id=2468, Sno="153", Model="16型", Address="安装位置", BatteryVoltage="27.6v", Status="正常" },
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
            new (){ Id=5678, PointNumber="153A04",PointName=" 瓦斯", InstallAddress="安装位置", PointValue= $"{Random.Shared.Next(1,5)}%", Status="异常" }
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
            new(){ Id=1357, Sno="152", Model="24型", Address="安装位置", BatteryVoltage=$"{Random.Shared.Next(20,30)}v", Status="正常" },
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
        httpContext.Response.Headers.Add("X-Accel-Buffering", "no"); // 禁用Nginx缓冲
        
        try
        {
            // 发送初始连接成功消息
            var initialMessage = Encoding.UTF8.GetBytes("data: {\"type\":\"connected\"}\n\n");
            await httpContext.Response.Body.WriteAsync(initialMessage, 0, initialMessage.Length);
            await httpContext.Response.Body.FlushAsync();
            
            // 使用CancellationTokenSource来管理连接
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(httpContext.RequestAborted);
            
            // 设置超时时间，防止连接无限期保持
            cts.CancelAfter(TimeSpan.FromHours(1));
            
            await foreach (var message in _channel.Reader.ReadAllAsync(cts.Token))
            {
                if (httpContext.RequestAborted.IsCancellationRequested || cts.Token.IsCancellationRequested)
                {
                    break;
                }
                
                var jsonMessage = JsonConvert.SerializeObject(message, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                
                var dataPage = Encoding.UTF8.GetBytes($"data: {jsonMessage}\n\n");
                await httpContext.Response.Body.WriteAsync(dataPage, 0, dataPage.Length);
                await httpContext.Response.Body.FlushAsync();
                
                // 添加日志以便调试
                Log.Information($"SSE消息已发送: {jsonMessage}");
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("SSE连接已取消");
        }
        catch (Exception ex)
        {
            Log.Error("响应SSE出错", ex);
        }
        finally
        {
            // 确保连接正确关闭
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 200;
            }
        }
    }
}
