namespace Furion.Demo.Application.Monitor.Dtos;

public class StationMonitorDto
{
    public long Id { get; set; }

    /// <summary>
    /// 分站号
    /// </summary>
    public string Sno { get; set; }

    /// <summary>
    /// 型号
    /// </summary>
    public string Model { get; set; }

    public string BatteryVoltage { get; set; }


    public string Address { get; set; }

    public string Status { get; set; }
}