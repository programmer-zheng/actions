using System.Web;
using Microsoft.AspNetCore.Mvc;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Interface;
using tik4net.Objects.Interface.Wireless;
using tik4net.Objects.Ip.DhcpServer;
using tik4net.Objects.Ip.Firewall;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Ddns.Controllers;

public class RosController : AbpController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _host;
    private readonly int _port;
    private readonly string _userName;
    private readonly string _password;

    public RosController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _host = configuration.GetValue<string>("Ros:Host")!;
        _port = configuration.GetValue<int>("Ros:Port")!;
        _userName = configuration.GetValue<string>("Ros:UserName")!;
        _password = configuration.GetValue<string>("Ros:Password")!;
    }


    [HttpGet("/EnableNatRule")]
    public IActionResult EnableNatRule(long port, string remoteIp = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(remoteIp))
            {
                var remoteIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;

                remoteIp = remoteIpAddress?.MapToIPv4().ToString();
                if (remoteIp.Equals("0.0.0.1"))
                {
                    remoteIp = string.Empty;
                }

                Logger.LogDebug(remoteIpAddress.ToString());
            }

            if (string.IsNullOrWhiteSpace(remoteIp))
            {
                return Json(new { Message = "无法启用nat规则，未能获取远程IP" });
            }

            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                connection.Open(_host, _port, _userName, _password);

                // 获取 NAT 规则
                var natRules = connection.LoadList<FirewallNat>();
                var natRule = natRules.FirstOrDefault(t => t.ToPorts == port);
                if (natRule != null)
                {
                    natRule.Disabled = false;
                    natRule.SrcAddress = remoteIp;
                    connection.Save(natRule);
                }
            }

            return Json(new
            {
                Message = "已启用nat规则"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "启用nat规则失败");
            return Json(new
            {
                Message = $"启用nat规则失败:{ex.Message}"
            });
        }
    }

    [HttpGet("/DisableNatRule")]
    public IActionResult DisableNatRule(long port)
    {
        try
        {
            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                connection.Open(_host, _port, _userName, _password);

                // 获取 NAT 规则
                var natRules = connection.LoadList<FirewallNat>();
                var natRule = natRules.FirstOrDefault(t => t.ToPorts == port);
                if (natRule != null)
                {
                    // 禁用匹配的 NAT 规则
                    natRule.Disabled = true;
                    connection.Save(natRule);
                }
            }

            return Json(new
            {
                Message = "已禁用nat规则"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "禁用nat规失败");
            return Json(new
            {
                Message = $"禁用nat规则失败:{ex.Message}"
            });
        }
    }

    [Route("/wifi")]
    public IActionResult WifiOnline()
    {
        try
        {
            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                connection.Open(_host, _port, _userName, _password);

                //查询wifi在线主机
                var onlineWifiHosts = connection.LoadList<WirelessRegistrationTable>();
                //查询dhcp主机
                var dhcpClients = connection.LoadList<DhcpServerLease>();

                var onlineHosts = (from client in dhcpClients
                        join wifiHost in onlineWifiHosts on client.MacAddress equals wifiHost.MacAddress
                        orderby wifiHost.Interface
                        select new
                        {
                            HostName = client.HostName,
                            Comment = client.Comment,
                            OnLineTime = wifiHost.Uptime.ToString(),
                            Interface = wifiHost.Interface,
                        }
                    ).ToList();

                return Json(onlineHosts);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "查看在线wifi列表失败");
            return Json(Enumerable.Empty<object>());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mac">远程启动设备的mac地址</param>
    /// <param name="intface"></param>
    /// <returns></returns>
    [Route("/wol")]
    public IActionResult Wol(string mac, string intface = "")
    {
        var msg = "已通知远程主机启动";

        #region 参数校验

        if (string.IsNullOrWhiteSpace(mac))
        {
            msg = "请提供远程设备的Mac地址";
        }

        #endregion

        try
        {
            mac = HttpUtility.UrlDecode(mac).Replace("-", ":");

            // 创建连接
            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                connection.Open(_host, _port, _userName, _password);

                if (!string.IsNullOrWhiteSpace(intface))
                {
                    connection.CallCommandSync($"/tool/wol interface = {intface} mac={mac}");
                }
                else
                {
                    var interfaces = connection.LoadList<Interface>();
                    foreach (var item in interfaces)
                    {
                        connection.CallCommandSync($"/tool/wol interface = {item} mac={mac}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "远程启动异常");
            msg = $"远程启动发生未知异常:{e.Message}";
        }

        return Json(new { Message = msg });
    }
}