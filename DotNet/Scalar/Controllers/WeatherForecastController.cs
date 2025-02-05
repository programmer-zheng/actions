using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Scalar.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 未来几天天气
    /// </summary>
    /// <param name="days">指定未来几天</param>
    /// <returns></returns>
    [HttpGet]
    [EndpointDescription("未来几天天气")]
    public IEnumerable<WeatherForecast> GetWeatherForecast([Required, Range(1, 10), Description("指定未来几天")] int days = 5)
    {
        return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet]
    [EndpointDescription("未来几天日期")]
    public IEnumerable<DateOnly> FutureDates([Required, Description("指定未来几天")] int days = 5)
    {
        if (days > 10)
        {
            throw new CustomException(5001, "只能查询未来10天以内日期");
        }

        return Enumerable.Range(1, days).Select(index => DateOnly.FromDateTime(DateTime.Now.AddDays(index)));
    }

    [HttpPost]
    public ModelBindTestDto ModelBindTest([FromBody] ModelBindTestDto model)
    {
        return model;
    }
}

public class ModelBindTestDto
{
    public string Name { get; set; }

    [Range(1, 200), DefaultValue(100)]
    public int Age { get; set; }


    public bool IsStudent { get; set; }
}