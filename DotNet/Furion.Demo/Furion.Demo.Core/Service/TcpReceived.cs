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
        System.Buffers.ArrayPool<byte> s_arrayPool = System.Buffers.ArrayPool<byte>.Shared;
        //var data = s_arrayPool.Rent(e.ByteBlock.CanReadLength);
        var data = new Byte[e.ByteBlock.CanReadLength];
        var cmdStr = e.ByteBlock[2].ToString("X");//命令
        var sno = e.ByteBlock[1].ToString("D3");//分站号
        if (cmdStr == "46")
        {

            //globalCollectionService.StationAttribuleList.FirstOrDefault(t => t.StationName == sno);
            Buffer.BlockCopy(e.ByteBlock.ToArray(), 0, data, 0, e.ByteBlock.CanReadLength);
            // 处理46命令
            // 这里可以添加具体的处理逻辑
            Console.WriteLine("Received command 46 from client.");
        }
        else
        {
            Console.WriteLine($"Received unknown command {cmdStr} from client.");
        }
        //var span = e.ByteBlock.ReadToSpan(e.ByteBlock.CanReadLength);
        /*var mes = e.ByteBlock.Span.ToString(Encoding.UTF8);
        client.Logger.Info($"已从{client.IP}接收到信息：{mes}");*/

        Console.WriteLine("----------收到TCP数据----------");
        await e.InvokeNext();
    }
}
