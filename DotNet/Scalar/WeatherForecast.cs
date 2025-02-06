using System.ComponentModel;

namespace Scalar;

public class WeatherForecast
{
    /// <summary>
    ///     日期
    /// </summary>

    [Description("日期")]
    public DateOnly Date { get; set; }

    /// <summary>
    ///     摄氏度
    /// </summary>
    [Description("摄氏度")]
    public int TemperatureC { get; set; }

    /// <summary>
    ///     华氏度
    /// </summary>
    [Description("华氏度")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    ///     描述
    /// </summary>
    [Description("描述")]
    public string? Summary { get; set; }
}