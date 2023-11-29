# Office365迁移测试
## 准备工作
[应用注册](https://docs.azure.cn/zh-cn/articles/azure-operations-guide/active-directory/aog-active-directory-howto-create-application-and-give-permission-by-postman#%E5%87%86%E5%A4%87%E5%B7%A5%E4%BD%9C)
### 所需权限
- Directory.Read.All
- Files.Read.All
- Group.Read.All
- GroupMember.Read.All
- Organization.Read.All
- OrgContact.Read.All
- Sites.Read.All (全球版)
- Sites.ReadWrite.All (中国区)
- User.Read.All


## 配置文件说明
- ThreadCount 设置线程数量
- TargetDownloadFileSize 设置下载文件总大小（单位字节）
- SharePointSetting 应用程序主要配置
  - TenantId 租户ID
  - ClientId 客户端ID
  - ClientSecret 客户端密钥
  - IsChina 是否中国区（true为中国区，false或不指定为全球）

## 执行 
``` bash
# 复制文件到服务器
scp Abp.MyConsoleApp root@192.168.1.3:/Office365
scp appsettings.json root@192.168.1.3:/Office365

# 登录服务器
ssh root@192.168.1.3

# 切换目录
cd /Office365

# 设置执行权限
chmod +x Abp.MyConsoleApp

# 执行程序
./Abp.MyConsoleApp 
``` 

## 登录
#### 中国区
- [Microsoft](https://portal.partner.microsoftonline.cn/Home)
- [Azure中国区主页](https://portal.azure.cn/#home)
- 账号： test@test0109.partner.onmschina.cn
- 密码： Jan49271

#### 全球
- [Microsoft Azure国际版主页](https://portal.azure.com/#home)

## 参考链接
https://developer.microsoft.com/en-us/graph/graph-explorer

https://developer.microsoft.com/en-us/graph/graph-explorer-china

