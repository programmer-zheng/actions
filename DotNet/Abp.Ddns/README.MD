### 项目用途
- DDNS更新域名解析，并将动态IP更新到阿里云安全组中
- 远程启动
- 查看在线设备
- 内网穿透在线列表

### Docker 镜像使用

``` bash
# 创建存放https证书的目录，并将证书文件上传进去
mkdir /https

# 创建docker容器，通过环境变量设置相关参数
# 需要注意：ASPNETCORE_URLS和ASPNETCORE_Kestrel__Certificates__Default__Password的值需要双引号
docker run -itd --name ddns --restart always \
-p 8880:80 -p 8843:443 \
-e Aliyun:AccessKeyId=keyid \
-e Aliyun:AccessKeySecret=keysecret \
-e Aliyun:SecurityGroupName=groupname \
-e Tencent:SecretId=secretid \
-e Tencent:SecretKey=secretkey \
-e Tencent:Domain=domain \
-e Tencent:Record=recordName \   
-e Tencent:RecordV6=recordName \
-e ASPNETCORE_URLS="https://+;http://+" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/xxxx.pfx \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="password_text" \
-v /https:/https \
programmerzheng/ddns
```
### Mikrotik中定时任务
#### IPv4 使用中转
``` bash
# 个人微信号
:local wxid "abcd"  

# 存储当前IP地址
:global currentIPv4;

#获取当前拨号的外网IPv4地址
:local nowIPv4 [/ip address get [find interface="pppoe-out1"] address];

# 判断当前IP与之前的IP是否一致
:if ($nowIPv4 != $currentIPv4) do={
    
    # 截取真实IP，去掉子网掩码
    :local ip [:pick $nowIPv4 0 [:find $nowIPv4 "/"]]
    
    # 设置nat转换，端口映射地址
    /ip fir nat set [find chain=dstnat] dst-address=$ip

    # 请求内部服务器中转
    /tool fetch http-method=get keep-result=no url="https://192.168.88.111:8843/ddns?ip=$ip"
                                  
    # 日志输出改变前后的IP
    :log info "ipv4 address $currentIPv4 changed to $nowIPv4";
    # 更新当前IP变量值
    :set currentIPv4 $nowIPv4;
    
    # 微信通知，参考 https://ros6.com/?action=show&id=238
    /tool fetch url="http://mail.ros6.com/wx/msg.php?act=sendmsg&wxid=$wxid&node=家里&msg=新的IP：$ip&lang=CN" output=none
}

```         
#### IPv4 直接请求DnsPod
``` bash
:global currentIPv4;

# 个人微信号
#DDNS Token
:local dnspodtoken "1234,abcd"
#DDNS域名
:local domain "domainName.com"
:local recordForIPv4 "www"
:local recordidForIPv4 "12345678"
:local wxid "abcd"

#获取当前拨号的外网IPv4地址
:local nowIPv4 [/ip address get [find interface="pppoe-out1"] address];

#判断当前IP与之前的IP是否一致
:if ($nowIPv4 != $currentIPv4) do={
    #截取真实IP，去掉子网掩码
    :local ip [:pick $nowIPv4 0 [:find $nowIPv4 "/"]]
    
    #设置nat转换，端口映射地址
    /ip fir nat set [find chain=dstnat] dst-address=$ip

    :local postdata "format=json&login_token=$dnspodtoken&domain=$domain&record_id=$recordidForIPv4&sub_domain=$recordForIPv4&record_line_id=0&value=$ip&ttl=120";
    #拼接请求地址
    #/tool fetch http-method=post keep-result=no http-header-field="content-type:application/x-www-form-urlencoded" url="https://dnsapi.cn/Record.Ddns" http-data="$postdata" 
                    
	#日志输出改变前后的IP
    :log info "ipv4 address $currentIPv4 changed to $nowIPv4";
    :set currentIPv4 $nowIPv4;

    /tool fetch url="http://mail.ros6.com/wx/msg.php?act=sendmsg&wxid=$wxid&node=家里&msg=新的IP：$ip&lang=CN" output=none
}

```

#### IPv6 使用中转
``` bash
# 个人微信号
:local wxid "abcd"

# 存储当前IP地址
:global currentIPv6;

#获取当前拨号的外网IPv4地址
:local nowIPv6 [/ipv6 address get [find interface="pppoe-out1" advertise=yes] address];

# 判断当前IP与之前的IP是否一致
:if ($nowIPv6 != $currentIPv6) do={
    # 截取真实IP，去掉子网掩码
    :local ip [:pick $nowIPv6 0 [:find $nowIPv6 "/"]]

    # 设置nat转换，端口映射地址
    # /ipv6 fir nat set [find chain=dstnat] dst-address=$ip

    # 请求内部服务器中转
    #/tool fetch http-method=get keep-result=no url="https://192.168.88.111:8843/ddns_ipv6?ip=$ip"

    # 日志输出改变前后的IP
    :log info "ipv6 address $currentIPv6 changed to $nowIPv6";

    # 更新当前IP变量值
    :set currentIPv6 $nowIPv6;

    # 微信通知，参考 https://ros6.com/?action=show&id=238
    /tool fetch url="http://mail.ros6.com/wx/msg.php?act=sendmsg&wxid=$wxid&node=家里&msg=新的IPv6：$ip&lang=CN" output=none
}

```         