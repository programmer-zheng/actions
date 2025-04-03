using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furion.Demo.Application.Monitor.Dtos;

public class CustomMonitorEventDto
{
    public object Data { get; set; }

    [Newtonsoft.Json.JsonConverterAttribute(typeof(StringEnumConverter))]
    public MonitorEventType Key { get; set; }

    private CustomMonitorEventDto() { }

    public CustomMonitorEventDto(MonitorEventType key, object data)
    {
        Key = key;
        Data = data;
    }
}