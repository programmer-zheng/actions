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
        
### 图标管理
[在线搜索图标 fontawesome v5 Free](https://fontawesome.com/v5/search?o=r&m=free)

## 菜单管理
### 添加菜单并设置权限
```csharp
// 只对主菜单添加
if (context.Menu.Name == StandardMenus.Main)
{
    // 获取管理菜单
    var administration = context.Menu.GetAdministration();
    // 管理菜单下添加一级菜单，并设置需要租户管理权限
    var firstLevel =
        new ApplicationMenuItem(StudyMenus.FirstLevel, l["Menu:FirstLevel"], icon: "fas fa-bars")
            .RequirePermissions(TenantManagementPermissions.Tenants.Default);

    administration.AddItem(firstLevel);

}    
```

## 多租户域名解析
在路由器中添加泛解析，将 `*.local.host` 解析到开发电脑，可成功绑定 asdf.local.host
```csharp
// WebModule 中配置如下
Configure<AbpTenantResolveOptions>(options =>
{
    options.AddDomainTenantResolver("{0}.local.host");
});
```
```json
// appsettings.json中配置监听地址
{ 
  "Kestrel": {
    "Endpoints": {
      "https": {
        "Url": "http://*:44363"
      }
    }
  }
}
```

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

# 升级故障
## abp 8
使用`.DbMigrator`项目进行迁移时，提示 `MySqlConnector.MySqlException (0x80004005): Unknown column 'o.ApplicationType' in 'field list'`
详细错误如下：
```text
E:/PersonalProject/actions/DotNet/Volo.Abp.Study/src/Volo.Abp.Study.DbMigrator/bin/Debug/net8.0/Volo.Abp.Study.DbMigrator.exe
[15:09:45 INF] Started database migrations...       
[15:09:45 INF] Migrating schema for host database...
[15:09:48 INF] Executing host database seed...
[15:09:50 ERR] Failed executing DbCommand (2ms) [Parameters=[@__ef_filter__p_0='?' (DbType = Boolean), @__clientId_0='?' (Size = 100)], CommandType='Text', CommandTimeout='30']
SELECT `o`.`Id`, `o`.`ApplicationType`, `o`.`ClientId`, `o`.`ClientSecret`, `o`.`ClientType`, `o`.`ClientUri`, `o`.`ConcurrencyStamp`, `o`.`ConsentType`, `o`.`CreationTime`, `o`.`CreatorId`, `o`.`DeleterId`, `o`.`DeletionTime`, `o`.`DisplayName`, `o`.`DisplayNames`, `o`.`ExtraProperties`, `o`.`IsDeleted`,
 `o`.`JsonWebKeySet`, `o`.`LastModificationTime`, `o`.`LastModifierId`, `o`.`LogoUri`, `o`.`Permissions`, `o`.`PostLogoutRedirectUris`, `o`.`Properties`, `o`.`RedirectUris`, `o`.`Requirements`, `o`.`Settings`
FROM `OpenIddictApplications` AS `o`
WHERE (@__ef_filter__p_0 OR NOT (`o`.`IsDeleted`)) AND (`o`.`ClientId` = @__clientId_0)
LIMIT 1
[15:09:50 ERR] An exception occurred while iterating over the results of a query for context type 'Volo.Abp.OpenIddict.EntityFrameworkCore.OpenIddictDbContext'.
MySqlConnector.MySqlException (0x80004005): Unknown column 'o.ApplicationType' in 'field list'
   at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
   at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConn
ector/MySqlDataReader.cs:line 483
   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56    
   at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
   at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
   at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
MySqlConnector.MySqlException (0x80004005): Unknown column 'o.ApplicationType' in 'field list'
   at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
   at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConn
ector/MySqlDataReader.cs:line 483
   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56    
   at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
   at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
   at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
   at Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync[TSource](IAsyncEnumerable`1 asyncEnumerable, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync[TSource](IAsyncEnumerable`1 asyncEnumerable, CancellationToken cancellationToken)
   at Volo.Abp.OpenIddict.Applications.EfCoreOpenIddictApplicationRepository.FindByClientIdAsync(String clientId, CancellationToken cancellationToken)
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous[TResult](IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapterWithReturnValue`1.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
   at Volo.Abp.Castle.DynamicProxy.CastleAsyncAbpInterceptorAdapter`1.InterceptAsync[TResult](IInvocation invocation, IInvocationProceedInfo proceedInfo, Func`3 proceed)
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.CreateApplicationAsync(String name, String type, String consentType, String displayName, String secret, List`1 grantTypes, List`1 scopes, String clientUri, String redirectUri, String postLogoutRedirectUri, List`1 permissions) in E:\PersonalProj
ect\actions\DotNet\Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 223
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.CreateApplicationsAsync() in Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 92
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.SeedAsync(DataSeedContext context) in Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 56
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous(IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapter.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
   at Volo.Abp.Castle.DynamicProxy.CastleAsyncAbpInterceptorAdapter`1.InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func`3 proceed)
   at Volo.Abp.Data.DataSeeder.SeedAsync(DataSeedContext context)
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous(IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapter.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
Unhandled exception. MySqlConnector.MySqlException (0x80004005): Unknown column 'o.ApplicationType' in 'field list'
   at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
   at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConn
ector/MySqlDataReader.cs:line 483
   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56    
   at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
   at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)
   at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
   at Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync[TSource](IAsyncEnumerable`1 asyncEnumerable, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.SingleOrDefaultAsync[TSource](IAsyncEnumerable`1 asyncEnumerable, CancellationToken cancellationToken)
   at Volo.Abp.OpenIddict.Applications.EfCoreOpenIddictApplicationRepository.FindByClientIdAsync(String clientId, CancellationToken cancellationToken)
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous[TResult](IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapterWithReturnValue`1.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
   at Volo.Abp.Castle.DynamicProxy.CastleAsyncAbpInterceptorAdapter`1.InterceptAsync[TResult](IInvocation invocation, IInvocationProceedInfo proceedInfo, Func`3 proceed)
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.CreateApplicationAsync(String name, String type, String consentType, String displayName, String secret, List`1 grantTypes, List`1 scopes, String clientUri, String redirectUri, String postLogoutRedirectUri, List`1 permissions) in E:\PersonalProj
ect\actions\DotNet\Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 223
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.CreateApplicationsAsync() in Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 92
   at Volo.Abp.Study.OpenIddict.OpenIddictDataSeedContributor.SeedAsync(DataSeedContext context) in Volo.Abp.Study\src\Volo.Abp.Study.Domain\OpenIddict\OpenIddictDataSeedContributor.cs:line 56
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous(IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapter.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
   at Volo.Abp.Castle.DynamicProxy.CastleAsyncAbpInterceptorAdapter`1.InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func`3 proceed)
   at Volo.Abp.Data.DataSeeder.SeedAsync(DataSeedContext context)
   at Castle.DynamicProxy.AsyncInterceptorBase.ProceedAsynchronous(IInvocation invocation, IInvocationProceedInfo proceedInfo)
   at Volo.Abp.Castle.DynamicProxy.CastleAbpMethodInvocationAdapter.ProceedAsync()
   at Volo.Abp.Uow.UnitOfWorkInterceptor.InterceptAsync(IAbpMethodInvocation invocation)
   at Volo.Abp.Castle.DynamicProxy.CastleAsyncAbpInterceptorAdapter`1.InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func`3 proceed)
   at Volo.Abp.Study.Data.StudyDbMigrationService.SeedDataAsync(Tenant tenant) in Volo.Abp.Study\src\Volo.Abp.Study.Domain\Data\StudyDbMigrationService.cs:line 103
   at Volo.Abp.Study.Data.StudyDbMigrationService.MigrateAsync() in Volo.Abp.Study\src\Volo.Abp.Study.Domain\Data\StudyDbMigrationService.cs:line 53
   at Volo.Abp.Study.DbMigrator.DbMigratorHostedService.StartAsync(CancellationToken cancellationToken) in Volo.Abp.Study\src\Volo.Abp.Study.DbMigrator\DbMigratorHostedService.cs:line 36
   at Microsoft.Extensions.Hosting.Internal.Host.StartAsync(CancellationToken cancellationToken)
   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)
   at Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(IHost host, CancellationToken token)
   at Volo.Abp.Study.DbMigrator.Program.Main(String[] args) in Volo.Abp.Study\src\Volo.Abp.Study.DbMigrator\Program.cs:line 30
   at Volo.Abp.Study.DbMigrator.Program.<Main>(String[] args)

进程已结束，退出代码为 -532,462,766。


```

使用迁移命令生成迁移记录时，提示 `System.TypeLoadException: Method 'Literal' in type 'Microsoft.EntityFrameworkCore.Design.Internal.CSharpHelper' from assembly 'Microsoft.EntityFrameworkCore.Design, Version=7.0.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60' does not have an implementation.`
详细如下：
``` text
Windows PowerShell
版权所有 (C) Microsoft Corporation。保留所有权利。

尝试新的跨平台 PowerShell https://aka.ms/pscore6

PS Volo.Abp.Study> dotnet ef migrations add udate-volo-abp-to-8 -s .\src\Volo.Abp.Study.Web\Volo.Abp.Study.Web.csproj -p .\src\Volo.Abp.Study.EntityFrameworkCore\Volo.Abp.Study.EntityFrameworkCore.csproj
Build started...
Build succeeded.
The Entity Framework tools version '7.0.13' is older than that of the runtime '8.0.0'. Update the tools for the latest features and bug fixes. See https://aka.ms/AAc1fbw for more information.
[15:11:11 INF] Starting web host.
System.TypeLoadException: Method 'Literal' in type 'Microsoft.EntityFrameworkCore.Design.Internal.CSharpHelper' from assembly 'Microsoft.EntityFrameworkCore.Design, Version=7.0.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60' does not have an implementation.
   at Microsoft.EntityFrameworkCore.Design.DesignTimeServiceCollectionExtensions.<>c__DisplayClass0_0.<AddEntityFrameworkDesignTimeServices>b__0(ServiceCollectionMap services)                                                                                         
   at Microsoft.EntityFrameworkCore.Infrastructure.EntityFrameworkServicesBuilder.TryAddProviderSpecificServices(Action`1 serviceMap)                                                                                                                                   
   at Microsoft.EntityFrameworkCore.Design.DesignTimeServiceCollectionExtensions.AddEntityFrameworkDesignTimeServices(IServiceCollection services, IOperationReporter reporter, Func`1 applicationServiceProviderAccessor)                                              
   at Microsoft.EntityFrameworkCore.Design.Internal.DesignTimeServicesBuilder.CreateServiceCollection(DbContext context)                                                                                                                                                
   at Microsoft.EntityFrameworkCore.Design.Internal.DesignTimeServicesBuilder.Build(DbContext context)                                                                                                                                                                  
   at Microsoft.EntityFrameworkCore.Design.Internal.MigrationsOperations.AddMigration(String name, String outputDir, String contextType, String namespace)                                                                                                              
   at Microsoft.EntityFrameworkCore.Design.OperationExecutor.AddMigrationImpl(String name, String outputDir, String contextType, String namespace)                                                                                                                      
   at Microsoft.EntityFrameworkCore.Design.OperationExecutor.AddMigration.<>c__DisplayClass0_0.<.ctor>b__0()                                                                                                                                                            
   at Microsoft.EntityFrameworkCore.Design.OperationExecutor.OperationBase.<>c__DisplayClass3_0`1.<Execute>b__0()                                                                                                                                                       
   at Microsoft.EntityFrameworkCore.Design.OperationExecutor.OperationBase.Execute(Action action)                                                                                                                                                                       
Method 'Literal' in type 'Microsoft.EntityFrameworkCore.Design.Internal.CSharpHelper' from assembly 'Microsoft.EntityFrameworkCore.Design, Version=7.0.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60' does not have an implementation.                          
PS Volo.Abp.Study> 

```
### 解决办法
将项目中 `EntityFrameworkCore` 相关的nuget包，升级到 `8.0.0`，问题解决