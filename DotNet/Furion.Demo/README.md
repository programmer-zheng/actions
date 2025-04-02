# Furion DEMO
测试Furion、SqlSugar、TDengine、MySQL  
- MySQL备份
- TDengine Code First创建数据库和脚本

## 项目初始化
### 脚手架安装
[furion--sqlsugar-脚手架安装](https://furion.net/docs/template#2722-furion--sqlsugar-%E8%84%9A%E6%89%8B%E6%9E%B6%E5%AE%89%E8%A3%85)
``` bash

# Mvc 模板
dotnet new install Furion.SqlSugar.Template.Mvc

# WebApi 模板
dotnet new install Furion.SqlSugar.Template.Api
```

### 使用脚手架
``` bash

# -n 指定项目名称 
# -f 指定.NET版本
dotnet new furionapi -n Furion.Demo -f net8
```


## 依赖环境Docker创建
### tdengine
``` bash

# Docker 创建tdengine 
docker run -itd --name tdengine --restart always -p 6030:6030 -p 6041:6041 -p 6043:6043 -p 6044-6049:6044-6049 -p 6044-6045:6044-6045/udp -p 6060:6060 tdengine/tdengine:3.3.2.0
```

## 仓储与ISqlSugarClient查询区别

### 仓储

``` c#
var list = await repository.AsQueryable()
    .WhereIF(!input.Sno.IsNullOrWhiteSpace(), t => t.SNO.Equals(input.Sno))
    .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
    .ToListAsync();
return list;
```



``` sql
SELECT `Id`,`SNO`,`PointType`,`PointNumber`,`PointValue`,`IsDeleted` 
FROM `Point_Data`  
WHERE  (`SNO` = @MethodConst0)   AND ( `IsDeleted` = @IsDeleted1 )
```

### ISqlSugarClient

``` c#
var list = await repository.Context.Queryable<PointEntity>()
   .WhereIF(!input.Sno.IsNullOrWhiteSpace(), t => t.SNO.Equals(input.Sno))
   .WhereIF(!input.PointNumber.IsNullOrWhiteSpace(), t => t.PointNumber.Equals(input.PointNumber))
   .Select(t => new { t.SNO, t.PointNumber, t.PointType, t.PointValue })
   .ToListAsync();
return list;
```



``` sql
SELECT  `SNO` AS `SNO` , `PointNumber` AS `PointNumber` , `PointType` AS `PointType` , `PointValue` AS `PointValue`  
FROM `Point_Data`  
WHERE  (`SNO` = @MethodConst0)   AND ( `IsDeleted` = @IsDeleted1 )
```