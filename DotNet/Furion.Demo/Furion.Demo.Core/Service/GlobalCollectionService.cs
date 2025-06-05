using Furion.Demo.Core.Entity;
using Furion.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Furion.Demo.Core.Service;

public class GlobalCollectionService : ISingleton
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GlobalCollectionService> _logger;
    private readonly IConfiguration _configuration;
    public GlobalCollectionService(IServiceScopeFactory scopeFactory,
        ILogger<GlobalCollectionService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        StationAttribuleList = new List<StationAttribute>();
        _logger = logger;
        _configuration = configuration;
    }
    public IList<StationAttribute> StationAttribuleList { get; set; }

    public async Task InitializeAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var services = scope.ServiceProvider;
            var db = services.GetService<ISqlSugarClient>();
            var stations = await db.Queryable<StationEntity>().ToListAsync();
            foreach (var item in stations)
            {
                var stationAttribule = new StationAttribute
                {
                    StationId = item.Id,
                    StationIp = item.ip,
                    StationPort = item.port,
                    StationName = item.sno
                };
                StationAttribuleList.Add(stationAttribule);
            }
        }
        catch (Exception e)
        {

            _logger.LogError(e, "初始化时发生异常");
        }
    }

    public async Task RunAllAsync()
    {

        try
        {
            var tasks = StationAttribuleList.Where(s => s.StationId != 8888888888888).Select(async station =>
            {
                try
                {
                    var success = await RunOneAsync(station).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing station {station.StationName}: {ex}");
                }
            }).ToList();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run all tasks: {ex}");
        }
    }

    private async Task<bool> RunOneAsync(StationAttribute stationAttribule)
    {
        try
        {
            await Task.Delay(1);
            stationAttribule.Client.socketClient = new TouchSocket.Sockets.TcpClient();
            // var myStationAttribute = new MyStationAttribute { Station = stationAttribule };
            var config = new TouchSocketConfig()
                .SetServerName(stationAttribule.StationName)
                .SetRemoteIPHost($"{stationAttribule.StationIp}:{stationAttribule.StationPort}")
                //.ConfigureContainer(
                //    a =>
                //    {
                //        a.AddConsoleLogger();
                //    }
                //)
                .ConfigurePlugins(a =>
                {
                    a.Add(new TcpReceived(stationAttribule));

                    // 发送46巡检数据
                    a.Add(new AccessRestrictionsPlugin(stationAttribule));

                });

            await stationAttribule.Client.socketClient.SetupAsync(config).ConfigureAwait(false);
            if (_configuration.GetValue<bool>("Collect"))
            {
                await stationAttribule.Client.socketClient.ConnectAsync().ConfigureAwait(false);
            }

            return true; // 成功连接
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, stationAttribule.StationName + "Run Err");
            /*stationAttribule.isSend = false;
            StationRunTypes.Add(new StationRunType { StationName = stationAttribule.StationName, isRun = false });
            Parallel.ForEach(stationAttribule.StationPoints.Simulations, simulation =>
            {
                // 修改每个对象的属性
                simulation.AlarmTime = DateTime.Now;
                simulation.StrValue = "断线";
                simulation.RealValue = -9999;
                simulation.PointDataOriginStatus = PointDataStatusEnum.Disconnect;
                simulation.PointDataStatus = PointDataStatusEnum.Disconnect;
            });

            Parallel.ForEach(stationAttribule.StationPoints.Switchs, simulation =>
            {
                // 修改每个对象的属性
                simulation.AlarmTime = DateTime.Now;
                simulation.StrValue = "断线";
                simulation.RealValue = -9999;
                simulation.PointDataOriginStatus = PointDataStatusEnum.Disconnect;
                simulation.PointDataStatus = PointDataStatusEnum.Disconnect;
            });*/
            return false; // 连接失败
        }
    }

}
