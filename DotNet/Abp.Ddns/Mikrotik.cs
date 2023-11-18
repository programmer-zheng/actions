using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Abp.Ddns;

    public class Mikrotik : IDisposable
    {
        #region 字段

        Stream connection;
        TcpClient con;

        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="userIpAddress"></param>
        /// <param name="port"></param>
        private void Init(string domain, string userIpAddress, int port, bool autoLogin, string username, string password)
        {
            con = new TcpClient();
            con.SendTimeout = 1000;
            con.ReceiveTimeout = 1000;
            try
            {
                con.Connect(domain, port);
            }
            catch (SocketException soe)
            {
                if (!string.IsNullOrWhiteSpace(userIpAddress))
                {
                    Init(userIpAddress, null, port, autoLogin, username, password);
                }
                else
                {
                    throw soe;
                }
            }
            connection = (Stream)con.GetStream();
            if (autoLogin)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentNullException("用户名为空，无法连接服务器!");
                }

                if (!Login(username, password))
                {
                    throw new ArgumentException("用户名或密码错误");
                }
            }
        }

        public Mikrotik(string domain, int port, string username, string password)
        {
            Init(domain, null, port, true, username, password);
        }

        /// <summary>
        /// 初始化ROS管理类，首先通过域名尝试连接ros，当失败时，尝试根据指定ip地址来连接，若再连接失败，则抛出异常
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="autoLogin"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public Mikrotik(string domain, string ip, int port, bool autoLogin, string username, string password)
        {
            Init(domain, ip, port, autoLogin, username, password);
        }

        #endregion

        #region Base Method or Help Method

        private Encoding GetGb2312Encoding()
        {

            //解决.net core无法获取GB2312问题
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encodeing = Encoding.GetEncoding("gb2312");
            return encodeing;
        }

        public void Close()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
            if (con != null)
            {
                con.Close();
                con = null;
            }
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Close();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        /// <summary>
        /// 登录ros，需要有读写权限
        /// </summary>
        /// <param name="username">ROS中管理员帐号</param>
        /// <param name="password">ROS中管理员密码</param>
        /// <returns></returns>
        private bool Login(string username, string password)
        {
            //username = StringUtils.DesDecrypt(username);
            //password = StringUtils.DesDecrypt(password);
            Send("/login", true);
            var strList = Read();
            var strArr = strList[0].Split(new string[] { "ret=" }, StringSplitOptions.None);
            string hash = strArr[1];
            Send("/login");
            Send("=name=" + username);
            Send("=response=00" + EncodePassword(password, hash), true);
            var data = Read();
            if (data[0] == "!done")
            {
                return true;
            }
            else
            {
                Send("/login");
                Send("=name=" + username);
                Send("=password=" + password, true);
                data = Read();
                if (data[0] == "!done")
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 向ROS发送请求
        /// </summary>
        /// <param name="co"></param>
        /// <param name="endsentence"></param>
        private void Send(string co, bool endsentence = false)
        {
            byte[] bajty = GetGb2312Encoding().GetBytes(co.ToCharArray());
            byte[] velikost = EncodeLength(bajty.Length);
            connection.Write(velikost, 0, velikost.Length);
            connection.Write(bajty, 0, bajty.Length);
            if (endsentence)
            {
                connection.WriteByte(0);
            }
        }

        /// <summary>
        /// 从ROS中获取返回信息
        /// </summary>
        /// <returns></returns>
        private List<string> Read()
        {
            List<string> output = new List<string>();
            List<byte> byteList = new List<byte>();
            string o = "";
            byte[] tmp = new byte[4];
            long count;
            while (true)
            {
                tmp[3] = (byte)connection.ReadByte();
                //if(tmp[3] == 220) tmp[3] = (byte)connection.ReadByte(); it sometimes happend to me that 
                //mikrotik send 220 as some kind of "bonus" between words, this fixed things, not sure about it though
                if (tmp[3] == 0)
                {
                    output.Add(GetGb2312Encoding().GetString(byteList.ToArray()));
                    if (o.Substring(0, 5) == "!done")
                    {
                        break;
                    }
                    else
                    {
                        byteList = new List<byte>();
                        o = "";
                        continue;
                    }
                }
                else
                {
                    if (tmp[3] < 0x80)
                    {
                        count = tmp[3];
                    }
                    else
                    {
                        if (tmp[3] < 0xC0)
                        {
                            int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[3], 0, 0 }, 0);
                            count = tmpi ^ 0x8000;
                        }
                        else
                        {
                            if (tmp[3] < 0xE0)
                            {
                                tmp[2] = (byte)connection.ReadByte();
                                int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[2], tmp[3], 0 }, 0);
                                count = tmpi ^ 0xC00000;
                            }
                            else
                            {
                                if (tmp[3] < 0xF0)
                                {
                                    tmp[2] = (byte)connection.ReadByte();
                                    tmp[1] = (byte)connection.ReadByte();
                                    int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[1], tmp[2], tmp[3] }, 0);
                                    count = tmpi ^ 0xE0000000;
                                }
                                else
                                {
                                    if (tmp[3] == 0xF0)
                                    {
                                        tmp[3] = (byte)connection.ReadByte();
                                        tmp[2] = (byte)connection.ReadByte();
                                        tmp[1] = (byte)connection.ReadByte();
                                        tmp[0] = (byte)connection.ReadByte();
                                        count = BitConverter.ToInt32(tmp, 0);
                                    }
                                    else
                                    {
                                        //Error in packet reception, unknown length
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    byte b = (byte)connection.ReadByte();
                    byteList.Add(b);
                    o += Encoding.GetEncoding("gb2312").GetString(EncodeLength(b));
                    //o += (Char)connection.ReadByte();
                }
            }
            return output;
        }

        byte[] EncodeLength(int delka)
        {
            if (delka < 0x80)
            {
                byte[] tmp = BitConverter.GetBytes(delka);
                return new byte[1] { tmp[0] };
            }
            if (delka < 0x4000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0x8000);
                return new byte[2] { tmp[1], tmp[0] };
            }
            if (delka < 0x200000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0xC00000);
                return new byte[3] { tmp[2], tmp[1], tmp[0] };
            }
            if (delka < 0x10000000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0xE0000000);
                return new byte[4] { tmp[3], tmp[2], tmp[1], tmp[0] };
            }
            else
            {
                byte[] tmp = BitConverter.GetBytes(delka);
                return new byte[5] { 0xF0, tmp[3], tmp[2], tmp[1], tmp[0] };
            }
        }

        /// <summary>
        /// 登录ROS时，对管理员密码进行编码加密
        /// </summary>
        /// <param name="Password"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        private string EncodePassword(string Password, string hash)
        {
            byte[] hash_byte = new byte[hash.Length / 2];
            for (int i = 0; i <= hash.Length - 2; i += 2)
            {
                hash_byte[i / 2] = Byte.Parse(hash.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            byte[] heslo = new byte[1 + Password.Length + hash_byte.Length];
            heslo[0] = 0;
            Encoding.ASCII.GetBytes(Password.ToCharArray()).CopyTo(heslo, 1);
            hash_byte.CopyTo(heslo, 1 + Password.Length);

            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            var hotovo = md5.ComputeHash(heslo);

            //Convert encoded bytes back to a 'readable' string
            string navrat = "";
            foreach (byte h in hotovo)
            {
                navrat += h.ToString("x2");
            }
            return navrat;
        }

        #endregion

        public List<HostInfo> GetWifiOnline()
        {
            var list = new List<HostInfo>();
            Send("/interface/wireless/registration-table/print");
            //Send("?disabled=no");//
            Send("=.proplist=mac-address,uptime,interface", true);//属性过滤，只显示名称  http://wiki.mikrotik.com/wiki/API#Command_description
            var macAddressList = Read();
            Send("/caps-man/registration-table/print");
            Send("=.proplist=mac-address,uptime,interface", true);
            macAddressList.AddRange(Read());
            Regex r = new Regex(@"mac-address=(?<mac>[^=]+).*uptime=(?<uptime>[^=]+).*interface=(?<interface>[^=]+)");
            Match m;
            foreach (var item in macAddressList)
            {
                m = r.Match(item);
                if (m.Success)
                {
                    list.Add(new HostInfo
                    {
                        MacAddress = m.Groups["mac"].Value,
                        OnLineTime = m.Groups["uptime"].Value,
                        Interface = m.Groups["interface"].Value,
                    });
                }
            }
            return list;
        }

        public List<HostInfo> GetDhcpHosts()
        {
            var list = new List<HostInfo>();
            Send("/ip/dhcp-server/lease/print");
            // Send("?status=bound");//状态为绑定
            Send("?disabled=no");//
            Send("=.proplist=mac-address,host-name,comment", true);//属性过滤，只显示名称  http://wiki.mikrotik.com/wiki/API#Command_description
            var macAddressList = Read();
            Regex regex = new Regex(@"mac-address=(?<mac>[^=]+).*(host-name=(?<hostname>[^=]+))?.*(comment=(?<comment>[^=]+))?");
            Regex hostname = new Regex(@"host-name=(?<hostname>[^=]+)");
            Regex comment = new Regex(@"comment=(?<comment>[^=]+)");
            Match m;
            foreach (var item in macAddressList)
            {
                m = regex.Match(item);
                if (m.Success)
                {
                    list.Add(new HostInfo
                    {
                        MacAddress = m.Groups["mac"].Value,
                        HostName = hostname.IsMatch(item) ? hostname.Match(item).Groups["hostname"].Value : string.Empty,
                        Comment = comment.IsMatch(item) ? comment.Match(item).Groups["comment"].Value : string.Empty,
                    }); ;
                }
            }
            return list;
        }

        /// <summary>
        /// 获取接口列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetInterfaceList()
        {
            var list = new List<string>();
            Send("/interface/print");
            Send("?type=ether");//查找类型为ether的
            Send("?disabled=no");//
            Send("=.proplist=name", true);//属性过滤，只显示名称  http://wiki.mikrotik.com/wiki/API#Command_description
            var etherInterfaces = Read();
            Regex r = new Regex(@"name=([^=]+)");
            Match m;
            foreach (var item in etherInterfaces)
            {
                m = r.Match(item);
                if (m.Success)
                {
                    list.Add(m.Groups[1].Value);
                }
            }
            return list;
        }

        /// <summary>
        /// 远程启动
        /// </summary>
        /// <param name="macAddress">远程设备的Mac地址</param>
        /// <returns></returns>
        public void Wol(string macAddress, string intface)
        {
            Send("/tool/wol");
            Send($"=interface={intface}");
            Send($"=mac={macAddress}");
            Send(".tag=wol", true);
            Read();
        }
    }

    public class HostInfo
    {
        [JsonIgnore]
        public string Address { get; set; }

        [JsonIgnore]
        public string MacAddress { get; set; }

        [JsonIgnore]
        public string Comment { get; set; }

        [JsonIgnore]
        public string HostName { get; set; }

        public string Name { get { return Comment == string.Empty ? HostName : Comment; } }

        public string OnLineTime { get; set; }

        /// <summary>
        /// 当前设备连接端口
        /// </summary>
        public string Interface { get; set; }

    }