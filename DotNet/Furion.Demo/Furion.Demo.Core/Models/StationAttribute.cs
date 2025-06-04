using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace Furion.Demo.Core;

public class StationAttribute
{
    public long StationId { get; set; }

    public string StationIp { get; set; }

    public int StationPort { get; set; }

    public string StationName { get; set; }

    public StationSocketClient Client { get; set; } = new StationSocketClient();
}

public class StationSocketClient
{
    public TcpClient socketClient { get; set; }
    public StationSocketClient() { }
}