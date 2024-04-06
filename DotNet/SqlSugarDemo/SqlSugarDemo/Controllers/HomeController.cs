using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    }

    public async Task<IActionResult> Index()
    {
        await db.Insertable<Student>(new List<Student>()
        {
            new Student() { Name = "test111" },
            new Student() { Name = "test222" },
        }).ExecuteCommandAsync();
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