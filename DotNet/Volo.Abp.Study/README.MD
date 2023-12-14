# volo abp学习项目
## abp环境
### 安装
``` bash
dotnet tool install -g Volo.Abp.Cli
```
### 更新
``` bash
dotnet tool update -g Volo.Abp.Cli
```

### 创建

``` bash
abp new Volo.Abp.Study -dbms MySQL -csf 
```
### 更新项目版本
```bash
# 解决方案根目录下运行
abp update
```

#### 数据库迁移步骤
- Web项目中添加`Microsoft.EntityFrameworkCore.Design` Nuget包引用
- 将Web项目设置为启动项目
- 在VS的`程序包管理器控制台`中，将默认项目设置为 `.EntityFrameworkCore` 
- 输入 `Add-Migration "UpdateTo742`
- 然后正常执行 `.Dbmigrator` 项目 或 在`程序包管理器控制台` 中输入 `Update-Database`

在`Rider`中可使用如下命令
``` bash
生成迁移记录
dotnet ef migrations add udate-volo-abp-to-742 -s .\src\Volo.Abp.Study.Web\Volo.Abp.Study.Web.csproj -p .\src\Volo.Abp.Study.EntityFrameworkCore\Volo.Abp.Study.EntityFrameworkCore.csproj


dotnet ef database update  -s .\src\Volo.Abp.Study.Web\Volo.Abp.Study.Web.csproj -p .\src\Volo.Abp.Study.EntityFrameworkCore\Volo.Abp.Study.EntityFrameworkCore.csproj
```

### 默认账号
- admin
- 1q2w3E*
                                              

### 代理
#### 动态代理


#### 静态代理

## JS相关
### 事件
- 保存成功
``` javascript

$(document).trigger("AbpSettingSaved");

// Volo.Abp.SettingManagement.Web中/Pages/SettingManagement/Index.js中的片段
$(document).on('AbpSettingSaved', function () {
    abp.notify.success(l('SuccessfullySaved'));

    abp.ajax({
       url: abp.appPath + 'SettingManagement?handler=RefreshConfiguration'
    });
}); 
```