using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace Furion.Demo.Core;
public class AccessRestrictionsPlugin : PluginBase, ITcpConnectingPlugin
{
    private StationAttribute stationAttribule;
    private Timer _timer;

    public AccessRestrictionsPlugin(StationAttribute stationAttribule)
    {
        this.stationAttribule = stationAttribule;
    }

    public Task OnTcpConnecting(ITcpSession client, ConnectingEventArgs e)
    {
        _timer = new Timer(SendData, client, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

        return Task.CompletedTask;
    }

    private async void SendData(object state)
    {
        var client = (ITcpClient)state;
        if (client.Online)
        {
            int sno = Convert.ToInt32(stationAttribule.StationName);
            byte hex = Convert.ToByte(sno.ToString("X2"), 16);

            if (client is ITcpClient tcpClient)
            {
                var command = CreateDataRequestCommand(hex, string.Empty);
                await tcpClient.SendAsync(command);

            }
        }
    }
    public new void Dispose()
    {
        _timer?.Dispose();
    }

    private const byte LEAD_BYTE = 0x7E;

    private byte[] CreateDataRequestCommand(byte stationAddress, string value = "")
    {
        var command = new List<byte>(6)
            {
                LEAD_BYTE,
                stationAddress,
                0x46,
                0,  // 初始化为0
                0,
                0   // 占位符，用于累加和
            };

        // 处理value参数
        if (!string.IsNullOrEmpty(value))
        {
            try
            {
                command[3] = Convert.ToByte(value, 16);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid hex value provided", nameof(value));
            }
        }

        // 计算校验和
        int sum = command.Skip(1).Take(command.Count - 3).Sum(b => b);
        command[^2] = (byte)(sum & 0xFF);
        command[^1] = (byte)(sum >> 8);

        return command.ToArray();
    }
}
