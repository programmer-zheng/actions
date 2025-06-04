using Furion.Demo.Core.Service;
using Furion.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Furion.Demo.Core;

public class TcpReceived : PluginBase, ITcpReceivedPlugin
{
    public TcpReceived(StationAttribute stationAttribule)
    {
        StationAttribule = stationAttribule;
    }

    public StationAttribute StationAttribule { get; }

    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        var receivedData = new Byte[e.ByteBlock.CanReadLength];
        Buffer.BlockCopy(e.ByteBlock.ToArray(), 0, receivedData, 0, e.ByteBlock.CanReadLength);

        var cmdStr = receivedData[2].ToString("X"); //命令
        var sno = receivedData[1].ToString("D3"); //分站号
        if (cmdStr == "46")
        {
            // 从分站响应中获取数据
            Console.WriteLine(string.Concat(receivedData.Select(x => x.ToString("X2"))) + "  " + sno + "end-----");
            // 处理46命令
            // 这里可以添加具体的处理逻辑
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd  HH:mm:ss} Received command 46 from {sno} client.");
        }
        else
        {
            Console.WriteLine($"Received unknown command {cmdStr} from client.");
        }
        //var span = e.ByteBlock.ReadToSpan(e.ByteBlock.CanReadLength);
        /*var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
        client.Logger.Info($"已从{client.IP}接收到信息：{mes}");*/

        await e.InvokeNext();
    }
}