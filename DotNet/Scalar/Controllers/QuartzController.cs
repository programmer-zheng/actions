using Microsoft.AspNetCore.Mvc;
using Quartz;
using Quartz.Impl;

namespace Scalar.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class QuartzController : ControllerBase
{
    [HttpGet]
    public async Task RecurringJob()
    {
        // 创建一个任务
        var job = JobBuilder.Create<MyJob>()
            .WithIdentity("MyJob", "Group1")
            .UsingJobData("id", 1234)
            .UsingJobData("name", "张三")
            .Build();

        // 创建一个延迟 10 分钟后执行的触发器
        var trigger = TriggerBuilder.Create()
            .WithIdentity("MyTrigger", "Group1")
            .StartNow() // 立即执行
            .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(10).RepeatForever()) // 每10秒执行一次，且一直重复
            .Build();

        // 获取调度器实例
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        // 调度任务
        await scheduler.ScheduleJob(job, trigger);
    }

    [HttpGet]
    public async Task RemoveJob()
    {
        var scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        // 假设你已经知道触发器的名称和组名
        var triggerKey = new TriggerKey("MyTrigger", "Group1");

        // 删除触发器
        await scheduler.UnscheduleJob(triggerKey);
    }
}

public class MyJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var id = (int)context.MergedJobDataMap["id"];
        var name = context.MergedJobDataMap["name"].ToString();
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Hello, Quartz! 参数：{id} -> {name}");
        return Task.CompletedTask;
    }
}