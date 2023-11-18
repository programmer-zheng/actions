### 项目用途
- DDNS更新域名解析
- 将动态IP更新到阿里云安全组中

### Docker 镜像使用
``` bash
docker run -itd --name ddns --restart always \
-p 8880:80 -p 8843:443 \
-e Aliyun:AccessKeyId=keyid \
-e Aliyun:AccessKeySecret=keysecret \
-e Aliyun:SecurityGroupName=groupname \
-e Tencent:SecretId=secretid \
-e Tencent:SecretKey=secretkey \
-e Tencent:Domain=domain \
-e Tencent:Record=record \
programmerzheng/ddns
```