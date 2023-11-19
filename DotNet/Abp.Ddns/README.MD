### 项目用途
- DDNS更新域名解析
- 将动态IP更新到阿里云安全组中

### Docker 镜像使用

``` bash
# 创建存放https证书的目录，并将证书文件上传进去
mkdir /https

# 创建docker容器，通过环境变量设置相关参数
docker run -itd --name ddns --restart always \
-p 8880:80 -p 8843:443 \
-e Aliyun:AccessKeyId=keyid \
-e Aliyun:AccessKeySecret=keysecret \
-e Aliyun:SecurityGroupName=groupname \
-e Tencent:SecretId=secretid \
-e Tencent:SecretKey=secretkey \
-e Tencent:Domain=domain \
-e Tencent:Record=record \          
-e ASPNETCORE_URLS="https://+;http://+" \
-e ASPNETCORE_Kestrel__Certificates__Default__Password="password_text" \
-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/xxxx.pfx \    
-v /https:/https \
programmerzheng/ddns
```
### Mikrotik中定时任务
``` bash
# 个人微信号
:local wxid "abcd"  

# 存储当前IP地址
:global currentIPv4;

#获取当前拨号的外网IPv4地址
:local nowIPv4 [/ip address get [find interface="pppoe-out1"] address];

# 判断当前IP与之前的IP是否一致
:if ($nowIPv4 != $currentIPv4) do={
    # 日志输出改变前后的IP
    :log info "ipv4 address $currentIPv4 changed to $nowIPv4";
    
    # 截取真实IP，去掉子网掩码
    :local ip [:pick $nowIPv4 0 [:find $nowIPv4 "/"]]
    
    # 设置nat转换，端口映射地址
    /ip fir nat set [find chain=dstnat] dst-address=$ip

    # 请求内部服务器中转
    /tool fetch http-method=get keep-result=no url="https://192.168.88.111:8843/ddns?ip=$ip"
    
    # 更新当前IP变量值
    :set currentIPv4 $nowIPv4;
    
    # 微信通知，参考 https://ros6.com/?action=show&id=238
    /tool fetch url="http://mail.ros6.com/wx/msg.php?act=sendmsg&wxid=$wxid&node=家里&msg=新的IP：$ip&lang=CN" output=none
}

```