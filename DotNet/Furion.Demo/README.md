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
部署完成后，将本地Cookie `TDengine-Token`的值改为`Basic%20cm9vdDp0YW9zZGF0YQ==`   
再访问`/explorer`页面，即可跳过登录界面的手机验证码，也无须登录

``` bash
# 也可以执行以下代码，跳过注册
# 方式一：直接将指定字符串写入到docker容器里面的文件
echo "http://buildkitsandbox:6041|18115181215|106001d6240e84aac2f32bea17060e24bf29a027" | docker exec -i tdengine sh -c 'cat > /etc/taos/explorer-register.cfg'

# 方式二：
# 1、进入容器
docker exec -it tdengine /bin/bash
# 2、创建文件
cat > /etc/taos/explorer-register.cfg << EOF
http://buildkitsandbox:6041|18115181215|106001d6240e84aac2f32bea17060e24bf29a027
EOF
```
``` bash

# Docker 创建tdengine 
docker run -itd --name tdengine --restart always -p 6030:6030 -p 6041:6041 -p 6043:6043 -p 6044-6049:6044-6049 -p 6044-6045:6044-6045/udp -p 6060:6060 tdengine/tdengine:3.3.3.0
```
[taosdump | TDengine 文档 | 涛思数据](https://docs.taosdata.com/2.6/reference/taosdump/)

#### 备份/恢复
``` bash
# 备份
taosdump -h localhost -P 6030 -D furiondemo_td -o D:\taos_backup
```

``` bash
# 恢复
taosdump -h localhost -P 6030 -i D:\taos_backup
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

## TdEngine
联合主键的形式，目前只有手动语句，sqlsugar暂不支持除时间戳外的其他主键  
即使加上了`[Key]`或`[SugarColumn(IsPrimaryKey = true)]`，生成的语句中也不会添加 `primary key`关键字
``` sql
CREATE STABLE IF NOT EXISTS  FURIONDEMO_TD.`point_data`(
`ts` TIMESTAMP   ,
`id`  INT  PRIMARY KEY ,
`pointtype` VARCHAR(255)   ,
`pointvalue` DOUBLE   ,
`day` VARCHAR(255) ) TAGS(`sno`  VARCHAR(100) ,`pointnumber`  VARCHAR(100));
```

``` sql

-- 可正常插入或更新
insert into xx using point_data tags('152','152A01') (ts,id,pointvalue) values('2025-04-08T11:42:23+08:00',1100,10);

-- 不管是插入还是更新，都会提示下方错误
-- DB error: Primary timestamp column should not be null
insert into xx using point_data tags('152','152A01') (ts,id,pointvalue) values('2025-04-08T11:42:23+08:00',1100,10);

```

