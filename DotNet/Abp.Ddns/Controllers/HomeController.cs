using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Abp.Ddns.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Interface;
using tik4net.Objects.Interface.Wireless;
using tik4net.Objects.Ip;
using tik4net.Objects.Ip.DhcpServer;
using tik4net.Objects.Ip.Firewall;
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


        /// <summary>
        /// 获取内网穿透在线列表
        /// </summary>
        /// <param name="address">frps服务地址（不包含http）</param>
        /// <param name="username">dashboard用户名</param>
        /// <param name="password">dashboard密码</param>
        /// <param name="port">dashboard端口</param>
        /// <returns></returns>
        [Route("/frps")]
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