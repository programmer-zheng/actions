using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlSugar;
using SqlSugarDemo.Models;

namespace SqlSugarDemo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public readonly ISqlSugarClient db;

    public HomeController(ILogger<HomeController> logger, ISqlSugarClient db)
    {
        _logger = logger;
        this.db = db;
        Task.Run(async () => { await InitData(); });
    }

    async Task InitData()
    {
        var studentList = new List<Student>
        {
            new Student() { Id = 1, Name = "张三" },
            new Student() { Id = 2, Name = "李四" },
            new Student() { Id = 3, Name = "王五" },
        };

        var books = new List<Book>
        {
            new() { Id = 1, SID = 1, BookName = "格林童话",  },// 不设置日期，默认C#初始化时，日期最小值为 0001-01-01，SqlSugar会将日期自动设置为 1900-01-01
            new() { Id = 2, SID = 1, BookName = "寓言故事", PublishDate = DateTime.Parse("1980-3-15") },
            new() { Id = 3, SID = 2, BookName = "C# 指南", PublishDate = DateTime.Parse("2020-4-20") },
        };

        await db.Storageable(studentList).ExecuteCommandAsync();
        await db.Storageable(books).ExecuteCommandAsync();
    }

    public async Task<IActionResult> Index()
    {
        var students = db.Queryable<Student>().Includes(t => t.Books).ToList();
        return Content(JsonConvert.SerializeObject(students), "application/json", Encoding.UTF8);
    }

    [Route("/test")]
    public async Task<IActionResult> Test()
    {
        var students = db.Queryable<Student>().Includes(t => t.Books).ToList();
        return Json(students);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}