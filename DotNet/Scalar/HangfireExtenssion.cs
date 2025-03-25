using Hangfire;

namespace Scalar;

public static class HangfireExtenssion
{
    
    public static void AddHangfireService(this IServiceCollection service)
    {
        service.AddHangfire(options =>
        {
            options.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();

            // options.UseLiteDbStorage(hfSqliteConnectionString);
            options.UseInMemoryStorage();
            // options.UseStorage(new MySqlStorage(hfMySqliteConnectionString, new MySqlStorageOptions()
            // {
            //     TransactionIsolationLevel = IsolationLevel.ReadCommitted,
            //     QueuePollInterval = TimeSpan.FromSeconds(15),
            //     JobExpirationCheckInterval = TimeSpan.FromHours(1),
            //     CountersAggregateInterval = TimeSpan.FromMinutes(5),
            //     PrepareSchemaIfNecessary = true,
            //     DashboardJobListLimit = 50000,
            //     TransactionTimeout = TimeSpan.FromMinutes(1),
            //     TablesPrefix = "Hangfire"
            // }));
        });


        service.AddHangfireServer(options =>
        {
            // options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
            // options.HeartbeatInterval = TimeSpan.FromMinutes(3);//设置服务器心跳间隔，每隔多久发送一次心跳
            // options.ServerTimeout = TimeSpan.FromMinutes(5);//设置服务器超时时间，超过这个时间，服务器会自动关闭,默认是5分钟
            // options.ServerCheckInterval = TimeSpan.FromMinutes(5);
            options.ServerName = "HangFireServer";
        });
    }
}