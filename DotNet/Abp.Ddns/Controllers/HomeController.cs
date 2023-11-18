using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Abp.Ddns.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Ddns.Controllers
{
    public class HomeController : AbpController
    {

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
                public IActionResult WifiOnline(string domain, string username, string password, int port)
        {
            using (var mikrotik = new Mikrotik(domain, port, username, password))
            {
                //查询wifi在线主机
                var onlineWifiHosts = mikrotik.GetWifiOnline();
                //查询dhcp主机
                var onlineHosts = mikrotik.GetDhcpHosts();
                onlineHosts = (from host in onlineHosts
                    join wifi in onlineWifiHosts on host.MacAddress equals wifi.MacAddress
                    select new HostInfo
                    {
                        HostName = host.HostName,
                        OnLineTime = wifi.OnLineTime,
                        Interface = wifi.Interface,
                        Comment = host.Comment,
                    }).ToList();
                return Json(onlineHosts);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domain">远程路由器的IP或域名</param>
        /// <param name="username">api用户名</param>
        /// <param name="password">api密码</param>
        /// <param name="port">api端口号</param>
        /// <param name="mac">远程启动设备的mac地址</param>
        /// <returns></returns>
        public IActionResult Wol(string domain, string username, string password, int port, string mac, string intface = "")
        {
            var msg = "已通知远程主机启动";

            #region 参数校验

            if (string.IsNullOrWhiteSpace(domain))
            {
                msg = "请提供远程路由器的IP或域名";
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                msg = "请提供远程路由器的用户名";
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                msg = "请提供远程路由器的密码";
            }

            if (port < 1)
            {
                msg = "请正确提供远程路由器的API端口号";
            }

            if (string.IsNullOrWhiteSpace(mac))
            {
                msg = "请提供远程设备的Mac地址";
            }

            #endregion

            try
            {
                domain = HttpUtility.UrlDecode(domain);
                username = HttpUtility.UrlDecode(username);
                password = HttpUtility.UrlDecode(password);
                mac = HttpUtility.UrlDecode(mac).Replace("-", ":");
                using (var mikrotik = new Mikrotik(domain, port, username, password))
                {
                    //var onlineHosts = mikrotik.GetOnlineHosts();
                    //if (onlineHosts.Exists(t => t.MacAddress.Equals(mac)))
                    //{
                    //    msg = "远程设备已在线，无须重复开启!";
                    //}
                    //else
                    //{
                    if (!string.IsNullOrWhiteSpace(intface))
                    {
                        mikrotik.Wol(mac, intface);
                    }
                    else
                    {
                        var intfaceList = mikrotik.GetInterfaceList();
                        foreach (var item in intfaceList)
                        {
                            mikrotik.Wol(mac, item);
                        }
                    }
                    //}
                }
            }
            catch (ArgumentException e)
            {
                msg = $"参数异常:{e.Message}";
            }
            catch (SocketException e)
            {
                msg = $"连接异常:{e.Message}";
            }
            catch (Exception e)
            {
                msg = $"未知异常:{e.Message}";
            }

            return Json(new { Message = msg });
        }

        /// <summary>
        /// 获取内网穿透在线列表
        /// </summary>
        /// <param name="address">frps服务地址（不包含http）</param>
        /// <param name="username">dashboard用户名</param>
        /// <param name="password">dashboard密码</param>
        /// <param name="port">dashboard端口</param>
        /// <returns></returns>
        public async Task<IActionResult> Frps(string address, string username, string password, int port)
        {
            var url = $"http://{address}:{port}/api/proxy/tcp";
            try
            {
                using (var httpclient = new HttpClient())
                {
                    var authenticationString = $"{username}:{password}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                    httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                    var response = await httpclient.GetAsync(url);
                    var responseStr = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseStr);
                    if (jObject.ContainsKey("proxies"))
                    {
                        var array = (JArray)jObject["proxies"];
                        var list = array.Where(t => t.Value<string>("status") == "online")
                            .Select(t => new
                            {
                                name = t.Value<string>("name"), port = t["conf"].Value<int>("remote_port")
                            }).ToList();
                        var onlineList = list.Select(t => $"{t.name}    {t.port}").ToList();
                        var result = string.Join(Environment.NewLine, onlineList);
                        return Content(result);
                    }
                }
            }
            catch (Exception e)
            {
                return Content($"未能获取frpc数据，{e.Message}");
            }

            return Content("无frpc数据");
        }
    }
}