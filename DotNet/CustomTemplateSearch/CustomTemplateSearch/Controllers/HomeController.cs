using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using CustomTemplateSearch.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CustomTemplateSearch.Models;

namespace CustomTemplateSearch.Controllers;

[Route("{action=TableFields}/{id?}")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult TableFields(string entityName = "")
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        var dataTableEntityTypes = types.Where(t => t.IsClass && !t.IsAbstract) // 非抽象类
            .Where(t => !t.GetCustomAttributes(typeof(TemplateIgnoreAttribute), true).Any()) // 标记了模板忽略
            .Where(t => t.GetCustomAttributes(typeof(TableAttribute), true).Any() // 标记了 Table 标签
                        || t.Name.EndsWith("Entity") // 以Entity结尾
                        || t.IsSubclassOf(typeof(BaseEntity) // BaseEntity的子类
                        )
            )
            .ToList();
        var result = new Dictionary<string, Dictionary<string, string>>();
        if (string.IsNullOrWhiteSpace(entityName))
        {
            dataTableEntityTypes = dataTableEntityTypes.Where(t => t.Name == entityName).ToList();
        }

        // 循环数据库实体
        foreach (var entityType in dataTableEntityTypes)
        {
            var tableName = entityType.Name;
            // 获取自定义表名
            var entityTableAttr = entityType.GetCustomAttribute<TableAttribute>();
            if (entityTableAttr != null)
            {
                tableName = entityTableAttr.Name;
            }

            // 获取字段（未标记模板忽略）
            var properties = entityType.GetProperties()
                .Where(t => !t.GetCustomAttributes(typeof(TemplateIgnoreAttribute), true).Any())
                .Where(t => t.PropertyType.IsPrimitive || t.PropertyType == typeof(string))
                .ToList();

            if (properties.Any())
            {
                var propertiesDictionary = new Dictionary<string, string>();
                foreach (var propertyInfo in properties)
                {
                    var propertyName = propertyInfo.Name;
                    var propertyDisplayName = propertyInfo.Name;
                    var displayNameAttribute = propertyInfo.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayNameAttribute != null)
                    {
                        propertyDisplayName = displayNameAttribute.DisplayName;
                    }

                    propertiesDictionary.Add(propertyName, propertyDisplayName);
                }

                result.Add(tableName, propertiesDictionary);
            }
        }

        return Json(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}