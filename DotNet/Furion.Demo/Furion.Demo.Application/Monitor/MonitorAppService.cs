using Furion.Demo.Application.Monitor.Dtos;
using Furion.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Collections.Concurrent;

namespace Furion.Demo.Application.Monitor;

public class MonitorAppService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly Channel<CustomMonitorEventDto> _channel;
    private readonly IEventPublisher _eventPublisher;
    private static readonly ConcurrentDictionary<string, HttpResponse> _activeConnections = new();

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
        var randomNumber = Math.Round(Random.Shared.NextDouble(), 2);
        var status = randomNumber < 0.5 ? "正常" : "异常";
        var data = new List<PointRealTimeDataDto>()
        {
            new (){ Id=5678, PointNumber="153A04",PointName=" 瓦斯", InstallAddress="安装位置", PointValue= $"{randomNumber}%", Status=status }
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
        var randomNumber = Random.Shared.Next(10, 30);
        var status = randomNumber < 15 ? "正常" : "异常";
        var data = new List<StationMonitorDto>()
        {
            new(){ Id=1357, Sno="152", Model="24型", Address="安装位置", BatteryVoltage=$"{randomNumber}v", Status=status },
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
    /// 广播消息到所有连接的客户端
    /// </summary>
    private async Task BroadcastMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tasks = new List<Task>();
        
        foreach (var connection in _activeConnections.Values)
        {
            try
            {
                tasks.Add(connection.Body.WriteAsync(messageBytes, 0, messageBytes.Length));
                tasks.Add(connection.Body.FlushAsync());
            }
            catch (Exception ex)
            {
                Log.Error("向客户端发送消息时出错", ex);
            }
        }
        
        await Task.WhenAll(tasks);
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
        
        // 设置SSE响应头
        httpContext.Response.Headers.Add("Content-Type", "text/event-stream");
        httpContext.Response.Headers.Add("Cache-Control", "no-cache");
        httpContext.Response.Headers.Add("Connection", "keep-alive");
        httpContext.Response.Headers.Add("X-Accel-Buffering", "no"); // 禁用Nginx缓冲

        // 生成连接ID并添加到活动连接列表
        var connectionId = Guid.NewGuid().ToString();
        _activeConnections.TryAdd(connectionId, httpContext.Response);
        
        try
        {
            // 发送初始连接成功消息
            var initialMessage = Encoding.UTF8.GetBytes("data: {\"type\":\"connected\"}\n\n");
            await httpContext.Response.Body.WriteAsync(initialMessage, 0, initialMessage.Length);
            await httpContext.Response.Body.FlushAsync();
            
            Log.Information($"新的SSE连接已建立，连接ID: {connectionId}，当前活动连接数: {_activeConnections.Count}");

            // 使用CancellationTokenSource来管理连接
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(httpContext.RequestAborted);

            // 设置超时时间，防止连接无限期保持
            cts.CancelAfter(TimeSpan.FromHours(24)); // 延长超时时间到24小时

            // 创建心跳定时器
            var heartbeatTimer = new Timer(async _ =>
            {
                try
                {
                    if (!httpContext.RequestAborted.IsCancellationRequested && !cts.Token.IsCancellationRequested)
                    {
                        var heartbeatMessage = "data: {\"type\":\"heartbeat\"}\n\n";
                        await BroadcastMessageAsync(heartbeatMessage);
                        Log.Debug("SSE心跳已发送到所有客户端");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("发送SSE心跳出错", ex);
                }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)); // 每30秒发送一次心跳

            // 注册取消回调，确保定时器被释放
            cts.Token.Register(() => heartbeatTimer.Dispose());

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

                var dataMessage = $"data: {jsonMessage}\n\n";
                await BroadcastMessageAsync(dataMessage);

                // 添加日志以便调试
                Log.Information($"SSE消息已广播到所有客户端: {jsonMessage}");
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information($"SSE连接已取消，连接ID: {connectionId}");
        }
        catch (Exception ex)
        {
            Log.Error($"响应SSE出错，连接ID: {connectionId}", ex);
        }
        finally
        {
            // 从活动连接列表中移除
            _activeConnections.TryRemove(connectionId, out _);
            Log.Information($"SSE连接已关闭，连接ID: {connectionId}，当前活动连接数: {_activeConnections.Count}");
            
            // 确保连接正确关闭
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 200;
            }
        }
    }
}
