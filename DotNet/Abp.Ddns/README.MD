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