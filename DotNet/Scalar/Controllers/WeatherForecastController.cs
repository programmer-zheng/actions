using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Scalar.Controllers
{
    [ApiController]
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
        [HttpGet("GetWeatherForecast")]
        [EndpointDescription("未来几天天气")]
        public IEnumerable<WeatherForecast> GetWeatherForecast([Required, Range(1, 10),Description("指定未来几天")] int days = 5)
        {
            return Enumerable.Range(1, days).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
        }

        [HttpGet("FutureDates")]
        [EndpointDescription("未来几天日期")]
        public IEnumerable<DateOnly> FutureDates([Required, Range(1, 10),Description("指定未来几天")] int days = 5)
        {
            return Enumerable.Range(1, days).Select(index => DateOnly.FromDateTime(DateTime.Now.AddDays(index)));
        }
    }
}