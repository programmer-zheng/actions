using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Scalar.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class HangfireController : ControllerBase
{
    private readonly IRecurringJobManager _recurringJobManager;


    public HangfireController(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    [HttpGet]
    public async Task RecurringJobAsync()
    {
        _recurringJobManager.AddOrUpdate("MyJob", () => Job(), "*/10 * * * * ?");
    }

    [HttpGet]
    public async Task RemoveJob()
    {
        _recurringJobManager.RemoveIfExists("MyJob");
    }

    [JobDisplayName("hangfire 定时任务")]
    [NonAction]
    public void Job()
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Hello Hangfire!");
    }
}