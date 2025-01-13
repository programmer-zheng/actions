namespace Scalar
{
    public class WeatherForecast
    {
        /// <summary>
        /// 日期 
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// 摄氏度
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// 华氏度
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// 描述
        /// </summary>
        public string? Summary { get; set; }
    }
}
